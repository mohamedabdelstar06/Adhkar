namespace Zad.MigrationRunner.Cli;

public static class RunnerUsage
{
    public const string Text = """
        Zad.MigrationRunner — the only program that changes a ZAD schema.

        Usage:
          dotnet run --project tools/Zad.MigrationRunner -- --target <env> [options]
          dotnet run --project tools/Zad.MigrationRunner -- --add-tenant <slug> --host <host>

        Migration options:
          --target      local | ci | staging | production        (required)
          --databases   catalog | shared | dedicated | jobs | all (default: all)
          --tenant      <slug>            migrate the one database that tenant lives on
          --module      <Name>[,<Name>]   restrict to these modules
          --dry-run                       print the idempotent script, open no connection
          --script      <path>            write one idempotent script, apply nothing
          --seed-dev                      seed the demo tenant (local only)

        Tenant options (one company, one subdomain):
          --add-tenant  <slug>            register and activate a tenant (default target: local)
          --host        <host>            the exact host that tenant answers on, e.g. acme.zad.com
          --language    <code>            default language, default ar
          --timezone    <id>              tenant time zone, default UTC

        Exit codes: 0 success, 1 a database failed, 2 bad invocation.
        """;
}
