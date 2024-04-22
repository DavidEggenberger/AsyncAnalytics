﻿using Modules.TenantIdentity.IntegrationEvents;
using Shared.Features.Messaging.IntegrationEvent;

namespace Modules.Subscriptions.Features.DomainFeatures.StripeCustomerAggregate.Aplication.IntegrationEventHandlers
{
    public class UserUpdatedIntegrationEventHandler : IIntegrationEventHandler<UserEmailUpdatedIntegrationEvent>
    {
        public async Task HandleAsync(UserEmailUpdatedIntegrationEvent userEmailUpdatedIntegrationEvent, CancellationToken cancellationToken)
        {

        }
    }
}
