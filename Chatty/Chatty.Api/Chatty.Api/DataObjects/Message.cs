namespace AdMaiora.Chatty.Api.DataObjects
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }

        public string Sender
        {
            get;
            set;
        }

        public DateTime SendDate
        {
            get;
            set;
        }
    }
}