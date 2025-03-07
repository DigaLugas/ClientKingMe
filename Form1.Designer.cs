namespace ClientKingMe
{
    partial class Form1
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxPartidas = new System.Windows.Forms.ComboBox();
            this.detalhesPartida = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nomePartida = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.senhaPartida = new System.Windows.Forms.TextBox();
            this.criarPartida = new System.Windows.Forms.Button();
            this.entrarPartida = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.senhaPartidaEntrar = new System.Windows.Forms.TextBox();
            this.nomeJogador = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(253, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Selecionar Partida:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // comboBoxPartidas
            // 
            this.comboBoxPartidas.FormattingEnabled = true;
            this.comboBoxPartidas.Location = new System.Drawing.Point(253, 183);
            this.comboBoxPartidas.Name = "comboBoxPartidas";
            this.comboBoxPartidas.Size = new System.Drawing.Size(284, 21);
            this.comboBoxPartidas.TabIndex = 3;
            this.comboBoxPartidas.SelectedIndexChanged += new System.EventHandler(this.comboBoxPartidas_SelectedIndexChanged);
            // 
            // detalhesPartida
            // 
            this.detalhesPartida.Enabled = false;
            this.detalhesPartida.Location = new System.Drawing.Point(253, 223);
            this.detalhesPartida.Multiline = true;
            this.detalhesPartida.Name = "detalhesPartida";
            this.detalhesPartida.Size = new System.Drawing.Size(284, 201);
            this.detalhesPartida.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(24, 139);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(130, 24);
            this.label7.TabIndex = 5;
            this.label7.Text = "Criar Partida:";
            this.label7.Click += new System.EventHandler(this.label2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Monges de Clunny";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(130, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "DLL: 1.1";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // nomePartida
            // 
            this.nomePartida.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.nomePartida.Location = new System.Drawing.Point(28, 220);
            this.nomePartida.Name = "nomePartida";
            this.nomePartida.Size = new System.Drawing.Size(150, 23);
            this.nomePartida.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.label4.Location = new System.Drawing.Point(28, 189);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "Nome:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.label5.Location = new System.Drawing.Point(28, 275);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 20);
            this.label5.TabIndex = 10;
            this.label5.Text = "Senha:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(578, 122);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 13);
            this.label6.TabIndex = 11;
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // senhaPartida
            // 
            this.senhaPartida.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.senhaPartida.Location = new System.Drawing.Point(28, 306);
            this.senhaPartida.Name = "senhaPartida";
            this.senhaPartida.Size = new System.Drawing.Size(150, 23);
            this.senhaPartida.TabIndex = 12;
            // 
            // criarPartida
            // 
            this.criarPartida.Location = new System.Drawing.Point(28, 381);
            this.criarPartida.Name = "criarPartida";
            this.criarPartida.Size = new System.Drawing.Size(150, 43);
            this.criarPartida.TabIndex = 13;
            this.criarPartida.Text = "Criar partida";
            this.criarPartida.UseVisualStyleBackColor = true;
            this.criarPartida.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // entrarPartida
            // 
            this.entrarPartida.Location = new System.Drawing.Point(605, 380);
            this.entrarPartida.Name = "entrarPartida";
            this.entrarPartida.Size = new System.Drawing.Size(150, 43);
            this.entrarPartida.TabIndex = 14;
            this.entrarPartida.Text = "Jogar";
            this.entrarPartida.UseVisualStyleBackColor = true;
            this.entrarPartida.Click += new System.EventHandler(this.button2_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(602, 270);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(91, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Senha da partida:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(602, 323);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(91, 13);
            this.label9.TabIndex = 16;
            this.label9.Text = "Nome do jogador:";
            // 
            // senhaPartidaEntrar
            // 
            this.senhaPartidaEntrar.Location = new System.Drawing.Point(605, 286);
            this.senhaPartidaEntrar.Name = "senhaPartidaEntrar";
            this.senhaPartidaEntrar.Size = new System.Drawing.Size(150, 20);
            this.senhaPartidaEntrar.TabIndex = 17;
            // 
            // nomeJogador
            // 
            this.nomeJogador.Location = new System.Drawing.Point(605, 339);
            this.nomeJogador.Name = "nomeJogador";
            this.nomeJogador.Size = new System.Drawing.Size(150, 20);
            this.nomeJogador.TabIndex = 18;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Palatino Linotype", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(266, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(312, 87);
            this.label10.TabIndex = 19;
            this.label10.Text = "King Me!";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(602, 195);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(85, 13);
            this.label11.TabIndex = 20;
            this.label11.Text = "Nome da partida";
            this.label11.Click += new System.EventHandler(this.label11_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(601, 139);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(177, 24);
            this.label12.TabIndex = 21;
            this.label12.Text = "Entrar em Partida:";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 444);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.nomeJogador);
            this.Controls.Add(this.senhaPartidaEntrar);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.entrarPartida);
            this.Controls.Add(this.criarPartida);
            this.Controls.Add(this.senhaPartida);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nomePartida);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.detalhesPartida);
            this.Controls.Add(this.comboBoxPartidas);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxPartidas;
        private System.Windows.Forms.TextBox detalhesPartida;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox nomePartida;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button criarPartida;
        private System.Windows.Forms.TextBox senhaPartida;
        private System.Windows.Forms.Button entrarPartida;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox senhaPartidaEntrar;
        private System.Windows.Forms.TextBox nomeJogador;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
    }
}

