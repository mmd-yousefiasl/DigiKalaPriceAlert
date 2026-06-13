using Application.Dtos.NewsLetter.AdminSide;
using Application.Dtos.NewsLetter.ClientSide;
using Application.Dtos.SentLetter.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface INewsLetterService
    {
        Task<Result> ClientSideSignInNewsLetters(ClientSideSignInNewsLettersDto dto);

        Task<Result<AdminSideFilterNewsLettersDto>> AdminSideFilterNewsLetters(AdminSideFilterNewsLettersDto filter);

        Task<Result> AdminSideDeleteNewsLetter(int id);

        Task<Result> AdminSideSendNews(AdminSideSendLetterDto dto);

        Task<Result<List<string>>> AdminSideGetNewsLetterEmails();
    }
}
