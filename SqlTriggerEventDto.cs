using Anthill.CC.Domain.Entities.Events;
using Tech.Models.Enums;
using AutoMapper;

namespace Tech.Services.Common
{
    public class SqlTriggerEventDto
    {
        public string TableName { get; set; }

        public string KeyName { get; set; }

        public string KeyValue { get; set; }

        public OperationType OperationType { get; set; }
    }

    public class SqlTriggerEventDtoMapper : Profile
    {
        public SqlTriggerEventDtoMapper()
        {
            CreateMap<SqlTriggerEvent, SqlTriggerEventDto>();
        }
    }
}
