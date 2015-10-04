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
    
    public partial class Person : Page
    {
        public Person()
        {
            this.Awards = new HashSet<Award>();
            this.PersonComments = new HashSet<Comment>();
            this.SessionLogs = new HashSet<SessionLog>();
            this.Scenes = new HashSet<Scene>();
            this.ScenePosts = new HashSet<ScenePost>();
        }
    
        public bool IsDead { get; set; }
        public string Obiturary { get; set; }
        public Nullable<int> PlayerId { get; set; }
        public string CauseOfDeath { get; set; }
        public bool IsInMainParty { get; set; }
    
        public virtual ICollection<Award> Awards { get; set; }
        public virtual ICollection<Comment> PersonComments { get; set; }
        public virtual Player Player { get; set; }
        public virtual ICollection<SessionLog> SessionLogs { get; set; }
        public virtual ICollection<Scene> Scenes { get; set; }
        public virtual ICollection<ScenePost> ScenePosts { get; set; }
    }
}
