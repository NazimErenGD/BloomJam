using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Reflection;



[JsonConverter(typeof(ConditionJsonConverter))]
public interface ICondition
{
    bool Evaluate(DialogueContext context);
}

public class DialogueContext
{
    public bool HasFlag(GameFlags flag) => GameFlagsService.Has(flag);
}

public class DialogueEntry
{
    public string Id { get; set; }
    public List<DialogueLine> Lines { get; set; }
    public List<ICondition> Conditions { get; set; }
    public bool isOneTimeOnly { get; set; }
    public List<Choice> Choices { get; set; }
    // Flags to add to GameFlagsService when this entry plays.
    public List<string> SetFlags { get; set; }
    // Named actions to fire via DialogueActions when this entry plays.
    public List<DialogueAction> Actions { get; set; }
}

// Named callback fired through DialogueActions. Arg is optional and is
// passed verbatim to the handler (handlers parse it themselves if needed).
public class DialogueAction
{
    public string Key { get; set; }
    public string Arg { get; set; }
}

public class DialogueLine
{
    public string Speaker { get; set; }
    public string Text { get; set; }
    public string LineType { get; set; }
}

public class Choice
{
    public string Text { get; set; }
    public string ButtonColor { get; set; }
    public List<ICondition> Conditions { get; set; }
    public string NextDialogueId { get; set; }
    public bool isActive { get; set; }
    public List<ICondition> UnMetConditions { get; set; }
    public List<ICondition> BonusConditions { get; set; }
    // Named actions fired via DialogueActions when the player picks this choice.
    public List<DialogueAction> Actions { get; set; }
}


public class HasFlagCondition : ICondition
{
    [JsonIgnore]
    public GameFlags Flag { get; set; }

    public string FlagString { get; set; }

    public HasFlagCondition() { }

    public HasFlagCondition(GameFlags flag)
    {
        Flag = flag;
        FlagString = flag.ToString();
    }

    private void EnsureFlag()
    {
        if (Flag != GameFlags.None) return;
        if (string.IsNullOrEmpty(FlagString))
            throw new InvalidOperationException("HasFlagCondition has no FlagString.");

        if (!Enum.TryParse(FlagString, out GameFlags parsed))
            throw new InvalidOperationException($"Unknown GameFlags value: {FlagString}");

        Flag = parsed;
    }

    public bool Evaluate(DialogueContext context)
    {
        EnsureFlag();
        return context.HasFlag(Flag);
    }
}


public enum ComparisonOperator
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

public class AndCondition : ICondition
{
    public List<ICondition> Conditions { get; set; }

    public bool Evaluate(DialogueContext context)
    {
        foreach (var condition in Conditions)
            if (!condition.Evaluate(context)) return false;
        return true;
    }
}

public class OrCondition : ICondition
{
    public List<ICondition> Conditions { get; set; }

    public bool Evaluate(DialogueContext context)
    {
        foreach (var condition in Conditions)
            if (condition.Evaluate(context)) return true;
        return false;
    }
}

public class NotCondition : ICondition
{
    public ICondition Condition { get; set; }

    public bool Evaluate(DialogueContext context)
        => !Condition.Evaluate(context);
}

public class CountCondition : ICondition
{
    public List<ICondition> Conditions { get; set; }
    public ComparisonOperator Operator { get; set; }
    public float Value { get; set; }

    public bool Evaluate(DialogueContext context)
    {
        int trueCount = 0;
        foreach (var condition in Conditions)
            if (condition.Evaluate(context)) trueCount++;
        return Operator switch
        {
            ComparisonOperator.Equal         => trueCount == Value,
            ComparisonOperator.NotEqual      => trueCount != Value,
            ComparisonOperator.Greater       => trueCount > Value,
            ComparisonOperator.GreaterOrEqual=> trueCount >= Value,
            ComparisonOperator.Less          => trueCount < Value,
            ComparisonOperator.LessOrEqual   => trueCount <= Value,
            _ => false
        };
    }
}

public class ConditionJsonConverter : JsonConverter<ICondition>
{
    private static readonly Dictionary<string, Type> ConditionTypes = new()
    {
        { "HasFlagCondition", typeof(HasFlagCondition) },
        { "HasFlag",          typeof(HasFlagCondition) },
        { "AndCondition",     typeof(AndCondition) },
        { "OrCondition",      typeof(OrCondition) },
        { "NotCondition",     typeof(NotCondition) },
        { "CountCondition",   typeof(CountCondition) }
    };

    private static readonly Dictionary<Type, (PropertyInfo[] ConditionProperties, PropertyInfo[] OtherProperties)> PropertyCache = new();

    static ConditionJsonConverter()
    {
        foreach (var conditionType in ConditionTypes.Values)
        {
            GetOrCreatePropertyCache(conditionType);
        }
    }

    public override ICondition ReadJson(JsonReader reader, Type objectType, ICondition existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject jo = JObject.Load(reader);

        if (!jo.TryGetValue("Type", StringComparison.OrdinalIgnoreCase, out JToken typeToken))
        {
            throw new JsonSerializationException("Condition JSON is missing the 'Type' field.");
        }
        string typeName = typeToken.ToString();
        if (!ConditionTypes.TryGetValue(typeName, out var conditionType))
        {
            throw new JsonSerializationException($"Unknown Condition type: {typeName}");
        }

        var condition = (ICondition)Activator.CreateInstance(conditionType);

        serializer.Populate(jo.CreateReader(), condition);

        return condition;
    }

    private static (PropertyInfo[] ConditionProperties, PropertyInfo[] OtherProperties) GetOrCreatePropertyCache(Type conditionType)
    {
        if (PropertyCache.TryGetValue(conditionType, out var cachedProperties))
        {
            return cachedProperties;
        }

        var allProperties = conditionType.GetProperties();

        var conditionProperties = allProperties
            .Where(p => p.PropertyType == typeof(ICondition) ||
                       p.PropertyType == typeof(List<ICondition>))
            .ToArray();

        var otherProperties = allProperties
            .Where(p => p.PropertyType != typeof(ICondition) &&
                       p.PropertyType != typeof(List<ICondition>))
            .ToArray();

        var propertyInfo = (conditionProperties, otherProperties);

        PropertyCache[conditionType] = propertyInfo;

        return propertyInfo;
    }

    public override void WriteJson(JsonWriter writer, ICondition value, JsonSerializer serializer)
    {
        JObject jo = JObject.FromObject(value, serializer);
        jo.AddFirst(new JProperty("Type", value.GetType().Name.Replace("Condition", "")));
        jo.WriteTo(writer);
    }
}

public static class DialogueManager
{
    public static HashSet<string> ReadDialogueIds = new();
    public static List<DialogueEntry> AllDialogues = new();

    public static void LoadFromJson(string dialogueJsonName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "Dialogues", dialogueJsonName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError($"DialogueManager: Dialogue file not found at {path}");
            AllDialogues = new List<DialogueEntry>();
            return;
        }
        AllDialogues = JsonConvert.DeserializeObject<List<DialogueEntry>>(File.ReadAllText(path)) ?? new List<DialogueEntry>();
    }

    // Kept for backwards compatibility with the original API.
    public static void GetNewDialogue(string DialogueJsonName) => LoadFromJson(DialogueJsonName);

    public static DialogueEntry GetFirstMatchingDialogue(DialogueContext context)
    {
        foreach (var entry in AllDialogues)
        {
            if (IsDialogueValid(entry, context))
            {
                if (entry.isOneTimeOnly)
                    ReadDialogueIds.Add(entry.Id);
                ChoiceFactory(entry, context);
                return entry;
            }
        }
        return null;
    }

    public static DialogueEntry GetDialogueById(string id)
    {
        var entry = AllDialogues.FirstOrDefault(d => d.Id == id);
        return entry;
    }

    public static DialogueEntry GetRandomMatchingDialogue(DialogueContext context)
    {
        var validDialogues = AllDialogues.Where(entry => IsDialogueValid(entry, context)).ToList();
        if (validDialogues.Count == 0)
            return null;
        var randomDialogue = validDialogues[UnityEngine.Random.Range(0, validDialogues.Count)];
        if (randomDialogue.isOneTimeOnly)
        {
            ReadDialogueIds.Add(randomDialogue.Id);
        }
        ChoiceFactory(randomDialogue, context);
        return randomDialogue;
    }

    private static bool IsDialogueValid(DialogueEntry entry, DialogueContext context)
    {
        if (ReadDialogueIds.Contains(entry.Id))
        {
            return false;
        }

        if (entry.Conditions == null || entry.Conditions.Count == 0)
            return true;

        foreach (var condition in entry.Conditions)
        {
            if (!condition.Evaluate(context))
                return false;
        }
        return true;
    }

    public static void ChoiceFactory(DialogueEntry entry, DialogueContext context)
    {
        if (entry?.Choices == null) return;
        foreach (var choice in entry.Choices)
        {
            var conditions = choice.Conditions ?? new List<ICondition>();
            choice.isActive = conditions.All(condition => condition.Evaluate(context));
            choice.UnMetConditions = conditions.Where(condition => !condition.Evaluate(context)).ToList();
        }
    }
}
