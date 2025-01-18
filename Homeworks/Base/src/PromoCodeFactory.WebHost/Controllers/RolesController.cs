using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Роли
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
        /// Получить все доступные роли
        /// </summary>
        /// <returns>Возвращает список всех ролей</returns>
        /// <response code="200">Успешное выполнение</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleItemResponse>), (int)HttpStatusCode.OK)]
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
        /// Получить роль по её id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Вернёт найденую роль - объект EmployeeResponse</returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="404">Роль с заданным id не найдена</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoleItemResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RoleItemResponse>> GetRoleByIdAsync(Guid id)
        {
            var role = await _rolesRepository.GetByIdAsync(id);

            if (role == null)
                return NotFound("Не найдена роль с Id " + id.ToString());

            var roleModel = new RoleItemResponse()
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            };

            return roleModel;
        }


        /// <summary>
        /// Добавить новую роль
        /// </summary>
        /// <param name="id">GUID роли</param>
        /// <returns>Вернёт созданую роль - объект RoleItemResponse</returns>
        /// <response code="200">Успешное выполнение. Роль создана</response>
        /// <response code="400">Роль со сгенерированным БД id уже существует</response>
        [HttpPost]
        [ProducesResponseType(typeof(RoleItemResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
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
                return BadRequest("Уже есть роль с Id " + newGiud.ToString());

            var roleModel = new RoleItemResponse()
            {
                Id = roleNew.Id,
                Name = roleNew.Name,
                Description = roleNew.Description
            };
            //return roleModel;

            var routVar = new UriBuilder(Request.Scheme, Request.Host.Host, (int)Request.Host.Port, Request.Path.Value).ToString() + "/" + roleModel.Id.ToString();
            return Created(routVar, roleModel);
        }

        /// <summary>
        /// Обновить данные роли
        /// </summary>
        /// <param name="role">Данные роли - объект Role</param>
        /// <returns>Возвращает данные обновлённой роли - объект RoleItemResponse</returns>
        /// <response code="200">Успешное выполнение. Данные роли обновлены</response>        
        /// <response code="404">Не найдена роль с указаным id</response>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]

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
        /// <param name="id">id удаляемой роли</param>
        /// <returns>Данные далённой роли - объект RoleItemResponse</returns>
        /// <response code="200">Успешное выполнение. Роль удалёна</response>
        /// <response code="400">Роль присутствует в списке ролей одного из сотрудников (см. ответ для деталей)</response>
        /// <response code="404">Не найдена роль с указанным id</response>
        /// 
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<RoleItemResponse>> DeleteRoleAsync(Guid id)
        {
            var foundEmployerList = await GetEmployeeListByRoleIdAsync(id);

            if (foundEmployerList == null)
            {
                var deletedRole = await _rolesRepository.DeleteAsync(id);

                if (deletedRole == null)
                    return NotFound("Не найдена роль с Id = " + id.ToString());
                else
                    return Ok("Удалили роль с Id " + id.ToString());
            }
            else
            {
                ErrorResponse errorResponse = new ErrorResponse();
                errorResponse.ErrorMessage = "Удаляемая роль найдена у одного или нескольких сотрудников";
                errorResponse.Employees = foundEmployerList;

                return BadRequest(errorResponse);
            }
        }


        /// <summary>
        /// Получить список сотрудников с ролью по Id роли
        /// </summary>
        /// <returns>Возвращает список сотрудников с указанной ролью или пустой список, если не найдены</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("employees/{id:guid}")]
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