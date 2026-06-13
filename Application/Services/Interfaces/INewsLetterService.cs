using Application.Dtos.NewsLetter.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface INewsLetterService
    {
        Task<Result> ClientSideSignInNewsLetters(ClientSideSignInNewsLettersDto model);
    }
}
