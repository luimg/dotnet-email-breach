using Orleans.Runtime;

public interface IEmailGrain : IGrainWithStringKey
{
    Task<List<string>> GetEmailsByDomain();
    Task SetEmailsForDomain(List<string> emails);
}

public class EmailGrain : Grain, IEmailGrain
{
    private readonly IPersistentState<DomainEmailList> _state;

    public EmailGrain([PersistentState("email", "emailStore")] IPersistentState<DomainEmailList> emailState)
    {
        this._state = emailState;
    }

    public Task<List<string>> GetEmailsByDomain() => Task.FromResult(this._state.State.Emails);

    public async Task SetEmailsForDomain(List<string> emails)
    {
        this._state.State = new DomainEmailList() { Domain = this.GetPrimaryKeyString(), Emails = emails };
        await this._state.WriteStateAsync();
    }
}

[GenerateSerializer]
public record DomainEmailList
{
    public List<string> Emails { get; set; }
    public string Domain { get; set; }
}
