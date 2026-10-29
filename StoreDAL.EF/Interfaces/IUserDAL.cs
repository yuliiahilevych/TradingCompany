using System.Collections.Generic;
using StoreDTO;

namespace StoreDAL.EF.Interfaces
{
    public interface IUserDAL
    {
        UserDTO Create(UserDTO user);
        List<UserDTO> GetAll();
        UserDTO? GetById(int id);

        UserDTO? GetByLogin(string login);

        bool Update(UserDTO user);
        bool Delete(int id);
    }
}