using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Роли сотрудников
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRepository<Role> _rolesRepository;
        private readonly IRepository<Employee> _employeeRepository;

        public RolesController(IRepository<Role> rolesRepository, IRepository<Employee> employeeRepository)
        {
            _rolesRepository = rolesRepository;
            _employeeRepository = employeeRepository;
        }

        /// <summary>
        /// Получить все доступные роли сотрудников
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<RoleItemResponse>> GetRolesAsync()
        {
            var roles = await _rolesRepository.GetAllAsync();

            var rolesModelList = roles.Select(x =>
                new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList();

            return rolesModelList;
        }

        /// <summary>
        /// Добавить роль
        /// </summary>
        /// <returns>Возвращает объект созданной роли</returns>        
        [HttpPut]
        public async Task<ActionResult<RoleItemResponse>> CreateRoleAsync(RoleItemRequest roleVar)
        {
            Guid newGiud = Guid.NewGuid();
            Role newRole = new()
            {
                Id = newGiud,
                Name = roleVar.Name,
                Description = roleVar.Description
            };

            var roleNew = await _rolesRepository.CreateAsync(newRole);

            if (roleNew == null)
                return Conflict("Уже есть роль с Id " + newGiud.ToString());

            var roleModel = new RoleItemResponse()
            {
                Id = roleNew.Id,
                Name = roleNew.Name,
                Description = roleNew.Description
            };
            return roleModel;
        }

        /// <summary>
        /// Обновить роль
        /// </summary>
        /// <returns>Возвращает обновлённый объект</returns>
        [HttpPost]
        public async Task<ActionResult<RoleItemResponse>> UpdateRoleAsync(Role role)
        {
            var roleUpdated = (await _rolesRepository.UpdateAsync(role));

            if (roleUpdated == null)
                return NotFound("Не найдена роль с Id = " + role.Id.ToString());

            var roleModel = new RoleItemResponse()
            {
                Id = roleUpdated.Id,
                Name = roleUpdated.Name,
                Description = roleUpdated.Description
            };
            return roleModel;
        }


        /// <summary>
        /// Удалить роль
        /// </summary>
        /// <returns>Возвращает удалённый объект</returns>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<RoleItemResponse>> DeleteRoleAsync(Guid id)
        {
            var foundEmployerList = await GetEmployeeListByRoleIdAsync(id);

            if (foundEmployerList == null)
            {
                var deletedRole = await _rolesRepository.DeleteAsync(id);

                if (deletedRole == null)
                    return NotFound("Не найдена роль с Id = " + id.ToString());  // Не нашли роль с таким Id
                else
                    return Ok("Удалили роль с Id " + id.ToString());  // удалили
            }
            else
            {
                ErrorResponse errorResponse = new ErrorResponse();
                errorResponse.ErrorMessage = "Удаляемая роль найдена у одного или нескольких сотрудников";
                errorResponse.Employees = foundEmployerList;

                return Conflict(errorResponse);
            }
        }


        /// <summary>
        /// Получить список сотрудников с ролью по Id роли
        /// </summary>
        /// <returns>Возвращает список сотрудников с указанной ролью или пустой список, если не найдены такие</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("{id:guid}")]
        public async Task<IEnumerable<EmployeeShortResponse>> GetEmployeeListByRoleIdAsync(Guid id)
        {

            var allEmployeeList = await _employeeRepository.GetAllAsync();

            List<EmployeeShortResponse> foundEmployeeList = new List<EmployeeShortResponse>();

            foreach (var employee in allEmployeeList)
            {
                foreach (var role in employee.Roles)
                {
                    if (role.Id == id)
                    {
                        EmployeeShortResponse newItem = new EmployeeShortResponse();
                        newItem.Id = employee.Id;
                        newItem.FullName = employee.FullName;
                        newItem.Email = employee.Email;

                        foundEmployeeList.Add(newItem);
                    }
                }
            }
            if (foundEmployeeList.Count > 0)
            {
                return foundEmployeeList;
            }
            else
            {
                return null;
            }
        }

    }
}