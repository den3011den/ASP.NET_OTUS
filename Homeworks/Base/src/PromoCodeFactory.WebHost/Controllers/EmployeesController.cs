using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Сотрудники
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Role> _rolesRepository;

        public EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> rolesRepository)
        {
            _employeeRepository = employeeRepository;
            _rolesRepository = rolesRepository;
        }

        /// <summary>
        /// Получение списка всех сотрудников
        /// </summary>
        /// <returns>Возвращает список всех сотрудников</returns>
        /// <response code="200">Успешное выполнение</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<EmployeeShortResponse>), (int)HttpStatusCode.OK)]
        public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();

            List<EmployeeShortResponse> employeesModelList = employees.Select(x =>
                new EmployeeShortResponse()
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                }).ToList();
            return employeesModelList;
        }


        /// <summary>
        /// Получить сотрудника по его id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Вернёт найденого сотрудника - объект EmployeeResponse</returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="404">Сотрудник с заданным id не найден</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return NotFound("Не найден сотрудник с Id " + id.ToString());

            var employeeModel = new EmployeeResponse()
            {
                Id = employee.Id,
                Email = employee.Email,
                Roles = employee.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employee.FullName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            return employeeModel;
        }

        /// <summary>
        /// Добавить нового сотрудника
        /// </summary>
        /// <param name="id">GUID сотрудника</param>
        /// <returns>Вернёт созданого сотрудника - объект EmployeeResponse</returns>
        /// <response code="200">Успешное выполнение. Сотрудник создан</response>
        /// <response code="400">Сотрудник со сгенерированным БД id уже существует</response>
        /// <response code="404">Роль сотрудника с указанным id не найдена в справочнике ролей</response>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync(EmployeeRequest employee)
        {
            Guid newId = Guid.NewGuid();
            Employee newEmployeeVar = new()
            {
                Id = newId,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                AppliedPromocodesCount = employee.AppliedPromocodesCount,
                Roles = employee.Roles.Select(x => new Role() { Id = x, Name = "", Description = "" }).ToList()
            };

            foreach (var role in newEmployeeVar.Roles)
            {
                var foundRole = await _rolesRepository.GetByIdAsync(role.Id);
                if (foundRole == null)
                {
                    return NotFound("Роль нового сотрудника с Id " + role.Id.ToString() + " не найдена в справочнике ролей.");
                }
                else
                {
                    role.Name = foundRole.Name;
                    role.Description = foundRole.Description;
                }

            }

            var employeeNew = await _employeeRepository.CreateAsync(newEmployeeVar);

            if (employee == null)
                return BadRequest("Уже есть сотрудник с Id " + newEmployeeVar.Id.ToString());

            var employeeModel = new EmployeeResponse()
            {
                Id = employeeNew.Id,
                Email = employeeNew.Email,
                Roles = employeeNew.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employeeNew.FullName,
                AppliedPromocodesCount = employeeNew.AppliedPromocodesCount
            };
            //return employeeModel;

            var routVar = new UriBuilder(Request.Scheme, Request.Host.Host, (int)Request.Host.Port, Request.Path.Value).ToString() + "/" + employeeModel.Id.ToString();
            return Created(routVar, employeeModel);

        }

        /// <summary>
        /// Обновить данные сотрудника
        /// </summary>
        /// <param name="employee">Данные сотрудника - объект EmployeeUpdateRequest</param>
        /// <returns>Возвращает данные обновлённого сотрудника - объект EmployeeResponse</returns>
        /// <response code="200">Успешное выполнение. Данные сотрудника обновлены</response>
        /// <response code="400">Одна из ролей сотрудника не найдена в справочнике ролей</response>
        /// <response code="404">Не найден сотрудник с указанным id</response>
        /// 
        [HttpPut]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EmployeeResponse>> UpdateEmployeeAsync(EmployeeUpdateRequest employee)
        {

            List<Role> roleItems = new List<Role>();
            foreach (var roleId in employee.Roles)
            {
                var foundRole = await _rolesRepository.GetByIdAsync(roleId);
                if (foundRole == null)
                {
                    return BadRequest("Роль обновляемого сотрудника с Id " + roleId.ToString() + " не найдена в справочнике ролей");
                }
                else
                {
                    roleItems.Add(new Role { Id = foundRole.Id, Name = foundRole.Name, Description = foundRole.Name });
                }
            }

            Employee newEmployee = new Employee
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Roles = roleItems,
                AppliedPromocodesCount = employee.AppliedPromocodesCount
            };

            var employeeUpdated = await _employeeRepository.UpdateAsync(newEmployee);

            if (employeeUpdated == null)
                return NotFound("Сотрудник с Id " + employee.Id.ToString() + "не найден");

            var employeeModel = new EmployeeResponse()
            {
                Id = employeeUpdated.Id,
                Email = employeeUpdated.Email,
                Roles = employeeUpdated.Roles.Select(x => new RoleItemResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                }).ToList(),
                FullName = employeeUpdated.FullName,
                AppliedPromocodesCount = employeeUpdated.AppliedPromocodesCount
            };
            return employeeModel;
        }

        /// <summary>
        /// Удалить сотрудника
        /// </summary>
        /// <param name="id">id удаляемого сотрудника</param>
        /// <returns>Данные далённого сотрудника - объект EmployeeResponse</returns>        
        /// <response code="200">Успешное выполнение. Сотрудник удалён</response>        
        /// <response code="404">Не найден сотрудник с указанным id</response>
        /// 
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EmployeeResponse>> DeleteEmployeeAsync(Guid id)
        {
            var deletedEmployee = await _employeeRepository.DeleteAsync(id);

            if (deletedEmployee == null)
                return NotFound("Не найден сотрудник с Id " + id.ToString());
            else
                return Ok("Удалён сотрудник с Id " + id.ToString());
        }
    }
}