using Corretor.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Data;

public sealed class CorretorDbContext(DbContextOptions<CorretorDbContext> options) : DbContext(options)
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<FaixaEtaria> FaixasEtarias => Set<FaixaEtaria>();
    public DbSet<Simulacao> Simulacoes => Set<Simulacao>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<PosContrato> PosContratos => Set<PosContrato>();
    public DbSet<Historico> Historicos => Set<Historico>();
    public DbSet<Script> Scripts => Set<Script>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<PessoaFisica> PessoasFisicas => Set<PessoaFisica>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Dependente> Dependentes => Set<Dependente>();
    public DbSet<PessoaJuridica> PessoasJuridicas => Set<PessoaJuridica>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Lead");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).IsRequired();
            entity.Property(x => x.Telefone).IsRequired();
            entity.Property(x => x.WorkflowEtapa).HasConversion<string>().IsRequired();
        });

        modelBuilder.Entity<FaixaEtaria>(entity =>
        {
            entity.ToTable("FaixaEtaria");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Faixa).IsRequired();
            entity.HasIndex(x => x.LeadId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Simulacao>(entity =>
        {
            entity.ToTable("Simulacao");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DataEnvio).IsRequired();
            entity.HasIndex(x => x.LeadId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Documento>(entity =>
        {
            entity.ToTable("Documento");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).IsRequired();
            entity.Property(x => x.Papel).IsRequired();
            entity.Property(x => x.DataUpload).IsRequired();
            entity.HasIndex(x => x.LeadId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.ToTable("Contrato");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LeadId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PosContrato>(entity =>
        {
            entity.ToTable("PosContrato");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ContratoId);
            entity.HasOne<Contrato>().WithMany().HasForeignKey(x => x.ContratoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Historico>(entity =>
        {
            entity.ToTable("Historico");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Tipo).HasConversion<string>().IsRequired();
            entity.Property(x => x.Data).IsRequired();
            entity.HasIndex(x => x.LeadId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Script>(entity =>
        {
            entity.ToTable("Script");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Etapa).HasConversion<string>().IsRequired();
            entity.Property(x => x.Tipo).IsRequired();
            entity.Property(x => x.Mensagem).IsRequired();
            entity.HasData(
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Etapa = ScriptEtapa.CriacaoLead,
                    Tipo = "WhatsApp",
                    Mensagem = "Ola, recebi seu contato para cotacao de plano de saude. Vou confirmar alguns dados para encontrar as melhores opcoes."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111112"),
                    Etapa = ScriptEtapa.ConversaInicial,
                    Tipo = "PerguntasIniciais",
                    Mensagem = "Voce procura plano individual, familiar ou empresarial? Tambem preciso saber cidade, operadora atual e se ha preferencia de rede."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111113"),
                    Etapa = ScriptEtapa.FaixaEtaria,
                    Tipo = "FaixaEtaria",
                    Mensagem = "Para calcular corretamente, me envie a quantidade de vidas por faixa etaria e informe quem sera titular e quem sera dependente."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111114"),
                    Etapa = ScriptEtapa.EnvioSimulacao,
                    Tipo = "Envio",
                    Mensagem = "Segue a simulacao com as opcoes de plano. Veja valores, rede, coparticipacao e cobertura antes de escolher a melhor alternativa."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111115"),
                    Etapa = ScriptEtapa.RetornoContato,
                    Tipo = "FollowUp",
                    Mensagem = "Conseguiu avaliar a simulacao enviada? Posso te ajudar a comparar as opcoes e tirar duvidas sobre rede, carencia e valores."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111116"),
                    Etapa = ScriptEtapa.AprovacaoSimulacao,
                    Tipo = "Aprovacao",
                    Mensagem = "Perfeito, vamos seguir com a opcao escolhida. Vou iniciar a etapa de documentos para formalizar a proposta."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111117"),
                    Etapa = ScriptEtapa.Documentacao,
                    Tipo = "Documentos",
                    Mensagem = "Envie os documentos do titular, dependentes ou empresa conforme o caso, incluindo identificacao e comprovante de endereco."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111118"),
                    Etapa = ScriptEtapa.Contrato,
                    Tipo = "Assinatura",
                    Mensagem = "A proposta esta pronta para assinatura. Confira os dados do contrato e me avise quando concluir para acompanharmos a implantacao."
                },
                new Script
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111119"),
                    Etapa = ScriptEtapa.PosContrato,
                    Tipo = "BoasVindas",
                    Mensagem = "Contrato concluido. Vou acompanhar os proximos passos e te orientar sobre carteirinha, acesso ao aplicativo e uso do plano."
                });
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente", table =>
                table.HasCheckConstraint(
                    "CK_Cliente_PessoaFisicaOuPessoaJuridica",
                    "((`PessoaFisicaId` IS NOT NULL AND `PessoaJuridicaId` IS NULL) OR (`PessoaFisicaId` IS NULL AND `PessoaJuridicaId` IS NOT NULL))"));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.LeadId);
            entity.HasIndex(x => x.PessoaFisicaId);
            entity.HasIndex(x => x.PessoaJuridicaId);
            entity.HasOne<Lead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PessoaFisica>().WithMany().HasForeignKey(x => x.PessoaFisicaId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PessoaJuridica>().WithMany().HasForeignKey(x => x.PessoaJuridicaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PessoaFisica>(entity =>
        {
            entity.ToTable("PessoaFisica");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nome).IsRequired();
            entity.Property(x => x.Cpf).IsRequired();
        });

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.ToTable("Endereco");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Logradouro).IsRequired();
            entity.Property(x => x.Estado).IsRequired();
            entity.Property(x => x.Cidade).IsRequired();
            entity.Property(x => x.Cep).IsRequired();
            entity.HasIndex(x => x.ClienteId);
            entity.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Dependente>(entity =>
        {
            entity.ToTable("Dependente");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PessoaFisicaId);
            entity.HasOne<PessoaFisica>().WithMany().HasForeignKey(x => x.PessoaFisicaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PessoaJuridica>(entity =>
        {
            entity.ToTable("PessoaJuridica");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NomeEmpresa).IsRequired();
            entity.Property(x => x.Cnpj).IsRequired();
            entity.Property(x => x.IE).IsRequired();
        });
    }
}
