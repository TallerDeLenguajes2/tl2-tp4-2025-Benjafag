using Biblioteca;
using Microsoft.AspNetCore.Mvc;

public class AsignarPedidoDTO
{
  public int NroPedido { get; set; }
  public int IdCadete { get; set; }
}

public class CambiarEstadoPedidoDTO
{
  public int NroPedido { get; set; }
  public EstadoPedido EstadoPedido { get; set; }
}

[ApiController]
[Route("[controller]")]
public class CadeteriaController : ControllerBase
{
  private readonly Cadeteria _cadeteria;

  public CadeteriaController(Cadeteria cadeteria)
  {
    _cadeteria = cadeteria;
  }

  [HttpGet("pedidos")]
  public ActionResult<List<Pedido>> GetPedidos() => Ok(_cadeteria.ListadoPedidos);

  [HttpGet("cadetes")]
  public ActionResult<List<Cadete>> GetCadetes() => Ok(_cadeteria.ListadoCadetes);

  [HttpPost("AgregarPedido")]
  public IActionResult AgregarPedido([FromBody] Pedido pedido)
  {
    if (pedido == null || pedido.Cliente == null ||
        string.IsNullOrEmpty(pedido.Obs) ||
        string.IsNullOrEmpty(pedido.Cliente.Nombre) ||
        string.IsNullOrEmpty(pedido.Cliente.Direccion) ||
        string.IsNullOrEmpty(pedido.Cliente.DatosReferenciaDireccion) ||
        pedido.Cliente.Telefono == 0)
      return BadRequest("Faltan datos");

    if (_cadeteria.ListadoPedidos.Any(p => p.Nro == pedido.Nro))
      return Conflict($"Ya existe un pedido con el numero {pedido.Nro}");

    _cadeteria.DarAltaPedido(pedido.Obs, pedido.Cliente.Nombre, pedido.Cliente.Direccion, pedido.Cliente.Telefono, pedido.Cliente.DatosReferenciaDireccion);

    if (pedido.CadeteAsociado != 0 && _cadeteria.ListadoCadetes.Any(c => c.Id == pedido.CadeteAsociado))
      _cadeteria.AsignarPedido(pedido.Nro, pedido.CadeteAsociado);

    return Ok(_cadeteria.ListadoPedidos.Last());
  }

  [HttpPut("AsignarPedido")]
  public IActionResult AsignarPedido([FromBody] AsignarPedidoDTO info)
  {
    if (info.IdCadete == 0 || info.NroPedido == 0)
      return BadRequest("Faltan datos");

    if (!_cadeteria.ListadoCadetes.Any(c => c.Id == info.IdCadete))
      return NotFound($"No existe cadete con el id {info.IdCadete}");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == info.NroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {info.NroPedido}");

    if (pedido.CadeteAsociado != 0)
      return Conflict($"El pedido con el nro {info.NroPedido} ya se encuentra asignado");

    _cadeteria.AsignarPedido(info.NroPedido, info.IdCadete);
    return NoContent();
  }

  [HttpPut("CambiarEstadoPedido")]
  public IActionResult CambiarEstadoPedido([FromBody] CambiarEstadoPedidoDTO info)
  {
    if (info.NroPedido == 0)
      return BadRequest("Faltan datos");

    if (info.EstadoPedido != EstadoPedido.Realizado)
      return BadRequest($"Estado incorrecto");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == info.NroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {info.NroPedido}");

    if (pedido.Estado == EstadoPedido.Realizado)
      return Conflict($"El pedido {info.NroPedido} ya se encuentra realizado");

    if (pedido.CadeteAsociado == 0)
      return Conflict($"El pedido {info.NroPedido} no se encuentra asignado a ningun cadete");

    _cadeteria.PedidoCompletado(pedido.Nro);
    return NoContent();
  }

  [HttpPut("CambiarCadetePedido")]
  public IActionResult CambiarCadetePedido([FromBody] AsignarPedidoDTO info)
  {
    if (info.IdCadete == 0 || info.NroPedido == 0)
      return BadRequest("Faltan datos");

    if (!_cadeteria.ListadoCadetes.Any(c => c.Id == info.IdCadete))
      return NotFound($"No existe cadete con el id {info.IdCadete}");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == info.NroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {info.NroPedido}");

    if (pedido.Estado == EstadoPedido.Realizado)
      return Conflict($"El pedido {pedido.Nro} ya se encuentra realizado");

    _cadeteria.ReasignarPedido(pedido.Nro, info.IdCadete);
    return NoContent();
  }
}