using AutoMapper;
using Investager.Core.Dtos;
using Investager.Core.Models;

namespace Investager.Core.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Asset, AssetSummaryDto>()
            .ForMember(e => e.Key, o => o.MapFrom(s => $"{s.Exchange}:{s.Symbol}"));
    }
}
