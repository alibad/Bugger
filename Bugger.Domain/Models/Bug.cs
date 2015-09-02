using System;

namespace Bugger.Domain.Models
{
    /// <summary>
    /// The model class use to shows in the UI.
    /// </summary>
    public class Bug : IEquatable<Bug>
    {
        public Bug()
        {
            this.Type = BugType.Yellow;
        }
        
        public bool Equals(Bug other)
        {
            return this.ID == other.ID
                && this.Title == other.Title
                && this.Description == other.Description
                && this.AssignedTo == other.AssignedTo
                && this.State == other.State
                && this.ChangedDate == other.ChangedDate
                && this.CreatedBy == other.CreatedBy
                && this.Priority == other.Priority
                && this.Severity == other.Severity;
        }
        /// <summary>
        /// Gets or sets the ID of this bug.
        /// </summary>
        /// <value>
        /// The ID of this bug.
        /// </value>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets a string that describes the title of this bug
        /// </summary>
        /// <value>
        /// A string that describes the title of this bug.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a string that describes this bug.
        /// </summary>
        /// <value>
        /// A string that describes this bug.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of this bug..
        /// </summary>
        /// <value>
        /// The type of this bug.
        /// </value>
        public BugType Type { get; set; }

        /// <summary>
        /// Gets or sets the string value of the user who this bug currently be assigned to.
        /// </summary>
        /// <value>
        /// The string value of the user who this bug currently be assigned to.
        /// </value>
        public string AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets a string that describes the state of this bug.
        /// </summary>
        /// <value>
        /// A string that describes the state of this bug.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the System.DateTime object that represents the date and time that this
        /// bug was last changed.
        /// </summary>
        /// <value>
        /// The System.DateTime object that represents the date and time that this bug 
        /// was last changed.
        /// </value>
        public DateTime ChangedDate { get; set; }

        /// <summary>
        /// Gets or sets the string value of the user who created this bug.
        /// </summary>
        /// <value>
        /// The string value of the user who created this bug.
        /// </value>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets a string that describes the priority of this bug.
        /// </summary>
        /// <value>
        /// A string that describes the priority of this bug.
        /// </value>
        public string Priority { get; set; }

        /// <summary>
        /// Gets or sets a string that describes the severity of this bug.
        /// </summary>
        /// <value>
        /// A string that describes the severity of this bug.
        /// </value>
        public string Severity { get; set; }
    }
}
