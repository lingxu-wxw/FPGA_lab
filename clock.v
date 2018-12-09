module clock(clk, cpu_clk, mem_clk);

	input 		clk;
	output 		cpu_clk, mem_clk; 

	reg 			cpu_clk;
	
	assign		mem_clk = clk;
	
	initial 
		begin
			cpu_clk <= 0;
		end

	always @(posedge clk)
		begin 
			cpu_clk <= ~cpu_clk;
		end
	
endmodule
