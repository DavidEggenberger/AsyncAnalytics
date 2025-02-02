﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Shared.Features.Domain;

namespace Modules.Channels.Features.DomainFeatures.Channels
{
    public class Message : Entity
    {
        public Guid ChannelId { get; set; }
        public Channel Channel { get; set; }
        public DateTime TimeSent { get; set; }
        public string Text { get; set; }
        public bool HasDerivedTopic { get; set; }
        public string DerivedTopicId { get; set; }
        public bool Votable { get; set; }

        private List<Reaction> votes = new List<Reaction>();
        public List<Reaction> MakeMessageTopicVotes => votes;
        internal void AddVote(Reaction vote)
        {
            if (votes.Any(v => v.UserId == vote.UserId) is false)
            {
                votes.Add(vote);
            }
        }
        internal void RemoveVote(Reaction vote)
        {
            votes.Remove(votes.FirstOrDefault(v => v.UserId == vote.UserId));
        }
    }

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.OwnsMany(x => x.MakeMessageTopicVotes)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
