//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Warhammer.Core.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Session : Page
    {
        public Session()
        {
            this.Posts = new HashSet<Post>();
            this.PostOrders = new HashSet<PostOrder>();
            this.SessionLogs = new HashSet<SessionLog>();
        }
    
        public Nullable<System.DateTime> DateTime { get; set; }
        public Nullable<int> Length { get; set; }
        public bool IsClosed { get; set; }
        public bool IsTextSession { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsGmTurn { get; set; }
        public Nullable<System.DateTime> XpAwarded { get; set; }
        public bool GmIsSuspended { get; set; }
        public Nullable<int> GmId { get; set; }
    
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<PostOrder> PostOrders { get; set; }
        public virtual ICollection<SessionLog> SessionLogs { get; set; }
    }
}
