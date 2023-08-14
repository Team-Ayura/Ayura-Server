using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class UserService : IUserService
{
    private readonly IMongoCollection<User> _userCollection;

    public UserService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
    }

    public List<User> Get()
    {
        var filter = Builders<User>.Filter.Empty;
        return _userCollection.Find(filter).ToList();
    }

    public User Get(string id)
    {
        var filter = Builders<User>.Filter.Empty;
        filter = filter & Builders<User>.Filter.Eq(u => u.Id, id);
        return _userCollection.Find(filter).FirstOrDefault();
    }

    public User Create(User user)
    {
        _userCollection.InsertOne(user);
        return user;
    }

    public void Update(string id, User user)
    {
        var filter = Builders<User>.Filter.Empty;
        filter = filter & Builders<User>.Filter.Eq(u => u.Id, id);
        _userCollection.ReplaceOne(filter, user);
    }

    public void Remove(string id)
    {
        var filter = Builders<User>.Filter.Empty;
        filter = filter & Builders<User>.Filter.Eq(u => u.Id, id);
        _userCollection.DeleteOne(filter);
    }

    public User UserByEmail(string email)
    {
        var filter = Builders<User>.Filter.Empty;
        filter = filter & Builders<User>.Filter.Eq(u => u.Email, email);
        return _userCollection.Find(filter).FirstOrDefault();
    }
}