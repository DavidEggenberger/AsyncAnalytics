﻿using Microsoft.AspNetCore.Components;
using Modules.TenantIdentity.Shared.DTOs.Tenant;
using Shared.Client;
using Shared.Kernel.BuildingBlocks.Services.Http;

namespace Web.Client.Components.Dashboard.Pages
{
    public partial class Dashboard : BaseComponent
    {
        [Inject]
        public HttpClientService httpClientService { get; set; }
        public List<TenantDTO> Teams { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Teams = await httpClientService.GetFromAPIAsync<List<TenantDTO>>("api/Team/all");
        }    
    }
}
