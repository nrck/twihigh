using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public class PatchTweetQueue
    {
        /// <summary>
        /// Target tweet id
        /// </summary>
        public Guid TweetId { get; set; }

        /// <summary>
        /// Patch operation
        /// </summary>
        public IEnumerable<TweetPatchOperation> Operations { get; set; }

        public PatchTweetQueue()
        {
            Operations = Array.Empty<TweetPatchOperation>();
        }

        public PatchOperation[] GetPatchOperations()
        {
            var ope = Operations.Select(operation =>
            {
                return operation.Type switch
                {
                    PatchOperationType.Add => PatchOperation.Add(operation.Path, operation.JsonStringValue),
                    PatchOperationType.Remove => PatchOperation.Remove(operation.Path),
                    PatchOperationType.Replace => PatchOperation.Replace(operation.Path, operation.JsonStringValue),
                    PatchOperationType.Set => PatchOperation.Set(operation.Path, operation.JsonStringValue),
                    PatchOperationType.Increment => PatchOperation.Increment(operation.Path, long.Parse(operation.JsonStringValue)),
                    _ => throw new NotSupportedException(),
                };
            }).ToArray();

            return ope;
        }
    }

    public class TweetPatchOperation
    {
        public PatchOperationType Type { get; set; }
        public string Path { get; set; } = string.Empty;
        public string JsonStringValue { get; set; } = string.Empty;

        public static TweetPatchOperation Add(string path, object value) => new() { Path = path, Type = PatchOperationType.Add, JsonStringValue = JsonSerializer.Serialize(value).TrimStart('"').TrimEnd('"') };
        public static TweetPatchOperation Remove(string path) => new() { Path = path, Type = PatchOperationType.Remove };
        public static TweetPatchOperation Replace(string path, object value) => new() { Path = path, Type = PatchOperationType.Replace, JsonStringValue = JsonSerializer.Serialize(value).TrimStart('"').TrimEnd('"') };
        public static TweetPatchOperation Set(string path, object value) => new() { Path = path, Type = PatchOperationType.Set, JsonStringValue = JsonSerializer.Serialize(value).TrimStart('"').TrimEnd('"') };
        public static TweetPatchOperation Increment(string path, object value) => new() { Path = path, Type = PatchOperationType.Increment, JsonStringValue = JsonSerializer.Serialize(value).TrimStart('"').TrimEnd('"') };
    }
}
