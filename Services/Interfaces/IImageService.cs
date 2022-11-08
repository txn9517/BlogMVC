namespace BlogMVC.Services.Interfaces
{
    // actions for uploading an image
    public interface IImageService
    {
        // converts the file to a byte array
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        // converts the byte array to a file
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage);
    }
}
