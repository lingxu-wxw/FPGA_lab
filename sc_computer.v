/////////////////////////////////////////////////////////////
//                                                         //
// School of Software of SJTU                              //
//                                                         //
/////////////////////////////////////////////////////////////

//module sc_computer (resetn,clock,mem_clk, pc,inst,aluout,memout,imem_clk,dmem_clk,
//	clrn, out_port0, out_port1, in_port0, in_port1, mem_dataout, io_read_data);

module sc_computer (resetn,clock,mem_clk, out_port0, out_port1, out_port2, in_port0, in_port1, mem_dataout, io_read_data,
		led0, led1, led2, led3, led4, led5, led6, led7, led8, led9);
	
// output [31:0] pc,inst,aluout,memout;
//	output [31:0] pc;
// output        imem_clk,dmem_clk;
	
   input 			resetn,clock,mem_clk;
	//input 			clrn; 						//new
	input	 [31:0]	in_port0, in_port1;		//new
	output [31:0]  out_port0, out_port1, out_port2;   //new
	output [31:0]  mem_dataout;				//new
	output [31:0]  io_read_data;				//new
	
   wire   [31:0]  data;
	wire	 [31:0]	pc;
   wire           wmem; // 1all these "wire"s are used to connect or interface the cpu,dmem,imem and so on.
	
	wire 	 [31:0]  dataout;						//new
	wire 				write_datamem_enable;	//new
	wire 	 [31:0]	mem_dataout;				//new
	
	wire   [31:0]  inst,aluout,memout;		//new
	wire           imem_clk,dmem_clk;		//new
	
	assign 			clrn = resetn;
	
   sc_cpu cpu (clock,resetn,inst,memout,pc,wmem,aluout,data);          // CPU module.
	//	sc_cpu cpu (clock,resetn);
   sc_instmem  imem (pc,inst,clock,mem_clk,imem_clk);                  // instruction memory.
   sc_datamem  dmem (aluout,data,memout,wmem,clock,mem_clk,dmem_clk, clrn, out_port0, out_port1, out_port2, in_port0, in_port1, mem_dataout, io_read_data); // data memory.
	
	output       led0, led1, led2, led3, led4, led5, led6, led7, led8, led9;
	reg       	 led0, led1, led2, led3, led4, led5, led6, led7, led8, led9;
	
	always @ (in_port0[0]) 
	begin
		led0 = in_port0[0];
	end
	
	always @ (in_port0[1]) 
	begin
		led1 = in_port0[1];
	end
	
	always @ (in_port0[2]) 
	begin
		led2 = in_port0[2];
	end
	
	always @ (in_port0[3]) 
	begin
		led3 = in_port0[3];
	end
	
	always @ (in_port0[4]) 
	begin
		led4 = in_port0[4];
	end
	
	always @ (in_port1[0]) 
	begin
		led5 = in_port1[0];
	end

	always @ (in_port1[1]) 
	begin
		led6 = in_port1[1];
	end
	
	always @ (in_port1[2]) 
	begin
		led7 = in_port1[2];
	end
	
	always @ (in_port1[3]) 
	begin
		led8 = in_port1[3];
	end

	always @ (in_port1[4]) 
	begin
		led9 = in_port1[4];
	end
	
endmodule



