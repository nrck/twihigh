using Microsoft.Azure.Cosmos;

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
        public IEnumerable<(PatchOperationType Type, string Path, object Value)> Operations { get; set; }

        public PatchTweetQueue()
        {
            Operations = Array.Empty<(PatchOperationType Type, string Path, object Value)>();
        }

        public PatchOperation[] GetPatchOperations()
        {
            var ope = Operations.Select(operation =>
            {
                return operation.Type switch
                {
                    PatchOperationType.Add => PatchOperation.Add(operation.Path, operation.Value),
                    PatchOperationType.Remove => PatchOperation.Remove(operation.Path),
                    PatchOperationType.Replace => PatchOperation.Replace(operation.Path, operation.Value),
                    PatchOperationType.Set => PatchOperation.Set(operation.Path, operation.Value),
                    PatchOperationType.Increment => PatchOperation.Increment(operation.Path, (long)operation.Value),
                    _ => throw new NotSupportedException(),
                };
            }).ToArray();

            return ope;
        }

        public PatchTweetQueue AppendOperation(PatchOperationType type, string path, object value)
        {
            Operations = Operations.Append((type, path, value));
            return this;
        }
    }
}
