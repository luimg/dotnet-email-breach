using Orleans.Runtime;

public class EmailGrain : IGrain
{
    private readonly IPersistentState<EmailState> _email;

    public EmailGrain([PersistentState("email", "emailStore")] IPersistentState<EmailState> email)
    {
        this._email = email;
    }

    public Task<string> GetEmailAsync() => Task.FromResult(this._email.State.Email);

    public async Task SetEmailAsync(string email)
    {
        this._email.State.Email = email;
        await this._email.WriteStateAsync();
    }
}
