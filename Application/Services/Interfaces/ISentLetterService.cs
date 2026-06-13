using Application.Dtos.SentLetter.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISentLetterService
    {
        Task<Result<AdminSideFilterSentLettersDto>> AdminSideFilterSentLetters(AdminSideFilterSentLettersDto filter);

        Task<Result> AdminSideDeleteSentLetter(int id);

        Task<Result> AdminSideCreateNewSentLetter(AdminSideCreateNewSentLetterDto dto);
    }
}
