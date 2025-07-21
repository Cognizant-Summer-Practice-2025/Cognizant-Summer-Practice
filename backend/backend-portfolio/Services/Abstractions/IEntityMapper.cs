namespace backend_portfolio.Services.Abstractions
{
    public interface IEntityMapper<TEntity, TDto>
    {
        TDto MapToDto(TEntity entity);
        IEnumerable<TDto> MapToDtos(IEnumerable<TEntity> entities);
    }

    public interface IEntityMapper<TEntity, TCreateDto, TResponseDto, TUpdateDto>
    {
        TResponseDto MapToResponseDto(TEntity entity);
        IEnumerable<TResponseDto> MapToResponseDtos(IEnumerable<TEntity> entities);
        TEntity MapFromCreateDto(TCreateDto createDto);
        void UpdateEntityFromDto(TEntity entity, TUpdateDto updateDto);
    }
} 