namespace Rezare.AuditLog
{
    /// <summary>
    /// Identifies a stream of activity for audit events.
    /// </summary>
    public enum AuditStream
    {
        /// <summary>
        /// All legal agreement activity (license approvals, agreements, T/C agreements, etc).
        /// </summary>
        LegalAgreements,

        /// <summary>
        /// Users logging on to the system.
        /// </summary>
        LogOn,

        /// <summary>
        /// User security activity, such as changing email addresses or passwords.
        /// </summary>
        UserSecurity,

        /// <summary>
        /// User activity that needs to be audit logged.
        /// </summary>
        UserActivity,

        /// <summary>
        /// Audit activity not covered in other streams.
        /// </summary>
        General,

        /// <summary>
        /// Software statement activity that needs to be logged
        /// </summary>
        SoftwareStatement
    }
}
