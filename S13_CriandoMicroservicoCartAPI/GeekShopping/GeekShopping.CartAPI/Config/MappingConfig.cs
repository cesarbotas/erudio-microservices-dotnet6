using AutoMapper;

namespace GeekShopping.CartAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingCopnfig = new MapperConfiguration(config =>
            {
                //config.CreateMap<ProductVO, Product>();
                //config.CreateMap<Product, ProductVO>();
            });

            return mappingCopnfig;
        }
    }
}