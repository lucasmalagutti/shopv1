using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("v1/users")]
public class UserController : Controller
{
    [HttpGet]
    [Route("")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
    {
        var users = await context
            .Users
            .AsNoTracking()
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> GetById(
        [FromServices] DataContext context,
        int id)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        return Ok(user);
    }

    [HttpPost]
    [Route("")]
    [AllowAnonymous]
    //[Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Post(
        [FromServices] DataContext context,
        [FromBody] User model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            model.Role = "employee";
            context.Users.Add(model);
            await context.SaveChangesAsync();
            //escondendo senha
            model.Password = "";
            return Ok(model);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível criar o usuário ", ex.Message });
        }
    }
    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<dynamic>> Authenticate(
        [FromServices] DataContext context,
        [FromBody] User model)
    {
        var user = await context.Users.
        AsNoTracking().
        Where(x => x.Username == model.Username && x.Password == model.Password)
        .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Usuário ou senha inválidos" });

        var token = TokenService.GenerateToken(user);
        //escondendo senha
        user.Password = "";
        return new
        {
            user = user,
            token = token
        };
    }
    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Put(
            [FromServices] DataContext context,
            int id,
            [FromBody] User model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != model.Id)
            return NotFound(new { message = "Usuário não encontrado" });

        try
        {
            context.Entry(model).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return model;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível atualizar o usuário", ex.Message });
        }
    }
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "manager")]
    public async Task<ActionResult<User>> Delete(
        int id,
        [FromServices] DataContext context)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user != null)
            return NotFound(new { message = "Usuário não encontrado" });

        try
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return Ok(new { message = "Usuário excluído com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível excluir usuário", ex.Message });
        }
    }
}