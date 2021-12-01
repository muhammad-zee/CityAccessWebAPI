using AutoMapper;
using System.Collections.Generic;

namespace Web.Services.Helper
{
    public class AutoMapperHelper
    {
        public static TDestination MapSingleRow<TSource, TDestination>(TSource inputModel)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TSource, TDestination>(MemberList.Source));
            var mapper = config.CreateMapper();
            return mapper.Map<TSource, TDestination>(inputModel);
        }

        public static List<TDestination> MapList<TSource, TDestination>(List<TSource> inputModel)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.CreateMap<TSource, TDestination>(MemberList.Source);
            });
            var mapper = config.CreateMapper();
            return mapper.Map<List<TSource>, List<TDestination>>(inputModel);
        }
        public static ICollection<TDestination> MapCollection<TSource, TDestination>(ICollection<TSource> inputModel)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.CreateMap<TSource, TDestination>(MemberList.Source);
            });
            var mapper = config.CreateMapper();
            return mapper.Map<ICollection<TSource>, ICollection<TDestination>>(inputModel);
        }

        public static TOuterDest MapSingleChildList<TOuterSource, TOuterDest, TInnerSource, TInnerDest>(TOuterSource inputModel)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TOuterSource, TOuterDest>();
                cfg.CreateMap<TInnerSource, TInnerDest>(MemberList.Source);
            });
            //config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var dest = mapper.Map<TOuterSource, TOuterDest>(inputModel);
            return dest;
        }

        public static TOuterDest MapMultipleChildList<TOuterSource, TOuterDest, TInnerSource, TInnerDest, TInnerSource1, TInnerDest1>(TOuterSource inputModel)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TOuterSource, TOuterDest>();
                cfg.CreateMap<TInnerSource, TInnerDest>(MemberList.Source);
                cfg.CreateMap<TInnerSource1, TInnerDest1>(MemberList.Source);
            });
            //config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            var dest = mapper.Map<TOuterSource, TOuterDest>(inputModel);
            return dest;
        }
    }
}
