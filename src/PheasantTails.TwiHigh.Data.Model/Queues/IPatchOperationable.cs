using Microsoft.Azure.Cosmos;

namespace PheasantTails.TwiHigh.Data.Model.Queues
{
    public interface IPatchOperationable
    {
        public PatchOperation[] GetPatchOperations();
    }
}
