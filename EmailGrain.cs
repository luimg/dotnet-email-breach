using System.Diagnostics.CodeAnalysis;
using Orleans.Runtime;

public interface IEmailGrain : IGrainWithStringKey
{
    Task<string> GetEmail();
    void SetSavingTimer();
}

public class EmailGrain : Grain, IEmailGrain
{
    private readonly IPersistentState<EmailState> _state;
    private IDisposable _timer;

    public EmailGrain([PersistentState("email", "emailStore")] IPersistentState<EmailState> emailState)
    {
        this._state = emailState;
    }

    public Task<string> GetEmail() => Task.FromResult(this._state.State.Email);

    public void SetSavingTimer()
    {
        // set the timer
        this._timer = this.RegisterTimer(async (object param) =>
        {
            this._state.State = (EmailState)param;
            await this._state.WriteStateAsync();
        },
            new EmailState() { Email = this.GetPrimaryKeyString() },
            TimeSpan.FromSeconds(0),
            TimeSpan.FromMinutes(5)
        );
    }
}

[GenerateSerializer]
public record EmailState
{
    [DisallowNull]
    public string Email { get; set; }
}
