using Firebase.Storage;

namespace pinq.api.Services;

public class FirebaseStorageService : IStorageService
{
    private readonly string _bucket = "pinq-nure.appspot.com";
    private readonly FirebaseStorage _storage;

    public FirebaseStorageService() => _storage = new FirebaseStorage(_bucket);

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName) => await _storage.Child(fileName).PutAsync(fileStream);
}