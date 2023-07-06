namespace PheasantTails.TwiHigh.Functions.Core.Services
{
    public interface IImageProcesserService
    {
        byte[] TrimmingToSquare(in byte[] buffer);
    }
}