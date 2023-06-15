using Orleans.Runtime;

public interface IEmailGrain : IGrainWithStringKey
{
    Task<string> GetEmail();
    Task PersistEmail();
}

public class EmailGrain : Grain, IEmailGrain
{
    private readonly IPersistentState<EmailState> _state;

    public EmailGrain([PersistentState("email", "emailStore")] IPersistentState<EmailState> emailState)
    {
        this._state = emailState;
    }

    public Task<string> GetEmail() => Task.FromResult(this._state.State.Email);

    public async Task PersistEmail()
    {
        this._state.State = new EmailState() { Email = this.GetPrimaryKeyString() };
        await this._state.WriteStateAsync();
    }
}

[GenerateSerializer]
public record EmailState
{
    public string Email { get; set; }
}
