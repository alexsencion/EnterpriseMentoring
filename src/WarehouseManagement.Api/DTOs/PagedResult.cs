namespace WarehouseManagement.Api.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Data,
    int TotalCount,
    int TotalPages,
    int PageNumber,
    int PageSize
);
