﻿namespace MarkMpn.Sql4Cds.LanguageServer.ObjectExplorer.Contracts
{
    /// <summary>
    /// Parameters to the <see cref="CloseSessionRequest"/>.
    /// </summary>
    public class CloseSessionParams
    {
        /// <summary>
        /// The Id returned from a <see cref="CreateSessionRequest"/>. This
        /// is used to disambiguate between different trees. 
        /// </summary>
        public string SessionId { get; set; }
    }
}
