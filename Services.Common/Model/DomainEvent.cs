namespace Services.Common.Abstractions.Model;

public abstract record DomainEvent;

public record InvestorCreated(Guid UserId, string InvestorId) : DomainEvent;

public record AccountCreated(string InvestorId, ProductCode Product, string AccountId) : DomainEvent;

public record KycFailed(Guid UserId, Guid ReportId) : DomainEvent;

public record ApplicationCompleted(Guid ApplicationId) : DomainEvent;
// Adding as it is missing, even if it is a share library it is not a breaking change
public record RequestFailed(Guid ApplicationId) : DomainEvent;