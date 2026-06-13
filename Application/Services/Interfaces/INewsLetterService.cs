using Application.Dtos.NewsLetter.AdminSide;
using Application.Dtos.NewsLetter.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface INewsLetterService
    {
        Task<Result> ClientSideSignInNewsLetters(ClientSideSignInNewsLettersDto model);

        Task<Result<AdminSideFilterNewsLettersDto>> AdminSideFilterNewsLetters(AdminSideFilterNewsLettersDto filter);

        Task<Result> AdminSideDeleteNewsLetter(int id);
    }
}
