FINTRUST MVC

FinTrust es un proyecto grupal de gestión financiera desarrollado con ASP.NET Core MVC. Organiza los datos de personas, socios y cuentas, y relaciona los préstamos con sus cuotas, pagos y garantías.

INTEGRANTES
Deibi Frandulcardo Cruz Gomez
Nicolas Tabora Kim
Cristhian Arispe Vargas

REPOSITORIO
https://github.com/DeibiCruz/FinTrust-MVC

TECNOLOGÍAS
C#, ASP.NET Core MVC y .NET 10.
Entity Framework Core con proveedor SQL Server, versión 10.0.13.
SQL Server, vistas Razor, HTML, CSS, JavaScript, Bootstrap y jQuery.

ORGANIZACIÓN
FinTrust.Web/Controllers: acciones y consultas de cada módulo.
FinTrust.Web/Models: entidades del sistema.
FinTrust.Web/Data: contexto y configuración de la base de datos.
FinTrust.Web/ViewModels: datos preparados para las pantallas de consulta.
FinTrust.Web/Views: páginas Razor.
FinTrust.Web/wwwroot: estilos, scripts y recursos visuales.

MÓDULOS
Personas y socios: registro y mantenimiento de los datos personales y de afiliación.
Cuentas: mantenimiento de cuentas y su vinculación con socios.
Préstamos y cuotas: registro del préstamo y de su programación de pagos.
Garantías reales y garantes: relación de los préstamos con las garantías y personas que los respaldan.
Operaciones financieras y pagos de cuotas: registro de operaciones y montos aplicados a las cuotas.
Reporte de préstamos: consulta del estado de los préstamos y acceso al detalle de sus cuotas.

REQUISITOS PARA EJECUTAR
Instalar el SDK de .NET 10 y disponer de una instancia de SQL Server accesible.
La base FinTrustDB debe estar creada con un esquema compatible con FinTrustDbContext y con permisos para el usuario de conexión. Utilizar la base del proyecto preparada por el equipo.

EJECUCIÓN
1. Descargar el repositorio o clonarlo:
   git clone https://github.com/DeibiCruz/FinTrust-MVC.git
2. Entrar en la carpeta FinTrust-MVC/FinTrust.Web.
3. Configurar ConnectionStrings:FinTrustConnection en appsettings.json con el servidor y la base de datos del equipo donde se ejecutará.
   Ejemplo para desarrollo local con autenticación de Windows:
   Server=localhost;Database=FinTrustDB;Trusted_Connection=True;TrustServerCertificate=True;
   Si se utilizan credenciales SQL, mantener la contraseña fuera de los archivos publicados.
4. Restaurar dependencias:
   dotnet restore
5. Compilar:
   dotnet build
6. Iniciar:
   dotnet run
7. Abrir en el navegador la dirección que muestra la consola.

CONSULTAS
En /ReportesPrestamos se puede filtrar por número de préstamo, estado y socio. El listado presenta capital, cuotas y los importes programados, pagados y pendientes.
El total programado suma el capital y el interés de las cuotas. El total pagado suma los pagos aplicados. El saldo pendiente se calcula como total programado menos total pagado.
Desde el listado se accede al detalle de un préstamo, con sus cuotas ordenadas por número, vencimientos, capital, interés, pagos y estado. Un préstamo sin cuotas todavía no tiene un calendario sobre el que calcular estos totales.

DIAGRAMA DE DATOS
DER_FinTrust.mdj contiene el diagrama en StarUML. DER_FinTrust.png permite consultarlo como imagen. Las entidades aparecen en rectángulos, sus atributos en óvalos y sus relaciones en rombos, con las cardinalidades junto a las conexiones.
