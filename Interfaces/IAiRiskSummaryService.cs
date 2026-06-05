using Backend_Crop_Insurrance.DTOs.Farmer;
using Backend_Crop_Insurrance.Application.Common;
namespace Backend_Crop_Insurrance.Interfaces
{
    public interface IAiRiskSummaryService
    {
        Task<Result<AiRiskSummaryResponseDto>> GetRiskSummaryAsync(
            AiRiskSummaryRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
