module sc_datamem (addr,datain,dataout,we,clock,mem_clk,dmem_clk,
	clrn, out_port0, out_port1, out_port2, in_port0, in_port1, mem_dataout, io_read_data);
 
   input  [31:0]  addr;
   input  [31:0]  datain;
   
   input          we, clock,mem_clk;
   output [31:0]  dataout;
   output         dmem_clk;

	input 			clrn; 						//new
	input	 [31:0]	in_port0, in_port1;		//new
	output [31:0]  out_port0, out_port1, out_port2;   //new
	output [31:0]  mem_dataout;				//new
	output [31:0]  io_read_data;				//new
   
   wire           dmem_clk;    
   wire           write_enable; 
	
	wire 	 [31:0]  dataout;						//new
	wire 				write_datamem_enable;	//new
	wire 	 [31:0]	mem_dataout;				//new
	
   assign         write_enable = we & ~clock;   
   assign         dmem_clk = mem_clk & ( ~ clock) ; 
   
	assign 			write_datamem_enable = write_enable & (~addr[7]);		//new, attention
	assign 			write_io_output_reg_enable = write_enable & ( addr[7]); //new, attention

	mux2x32			mem_io_dataout_mux(mem_dataout, io_read_data, addr[7], dataout);
	// module mux2x32 (a0,a1,s,y);
	
	// when address[7]=0, means the access is to the datamem.
	// that is, the address space of datamem is from 000000 to 011111 word(4 bytes)
	
   lpm_ram_dq_dram  dram(addr[6:2],dmem_clk,datain,write_enable, mem_dataout);
   //lpm_ram_dq_dram  dram(addr[6:2],dmem_clk,datain,write_enable,dataout );

	// when address[7]=1, means the access is to the I/O space.
	// that is, the address space of I/O is from 100000 to 111111 word(4 bytes)
	
	io_output_reg	io_output_regx2(addr, datain, write_io_output_reg_enable, dmem_clk, clrn, out_port0, out_port1, out_port2);
	//module io_output_reg(addr, datain, write_io_enable, io_clk, clrn, out_port0, out_port1, out_port2);
	
	io_input_reg	io_input_regx2(addr, dmem_clk, io_read_data, in_port0, in_port1);
	// module io_input_reg (addr,io_clk,io_read_data,in_port0,in_port1);
	
endmodule 