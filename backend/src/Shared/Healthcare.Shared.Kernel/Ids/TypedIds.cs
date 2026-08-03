namespace Healthcare.Shared.Kernel.Ids;

/// <summary>Identifies a staff user account (Administration identity aggregate).</summary>
public sealed record UserId(Guid Value) : StronglyTypedId<Guid>(Value);

/// <summary>Identifies an authorization role (Administration role aggregate).</summary>
public sealed record RoleId(Guid Value) : StronglyTypedId<Guid>(Value);

/// <summary>Identifies a patient (Administration patient aggregate).</summary>
public sealed record PatientId(Guid Value) : StronglyTypedId<Guid>(Value);

/// <summary>Identifies a facility / care location (Administration facility aggregate).</summary>
public sealed record FacilityId(Guid Value) : StronglyTypedId<Guid>(Value);
