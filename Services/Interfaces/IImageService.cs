namespace BlogMVC.Services.Interfaces
{
    // actions for uploading an image
    public interface IImageService
    {
        // convert the file to a byte array
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        // convert the byte array to a file
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage);
    }
}
