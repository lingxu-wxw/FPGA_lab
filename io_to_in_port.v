module io_to_in_port (sw4,sw3,sw2,sw1,sw0,in_port);
	input sw0,sw1,sw2,sw3,sw4;
	output [31:0] in_port;
	
	assign in_port[0] = sw0;
	assign in_port[1] = sw1;
	assign in_port[2] = sw2;
	assign in_port[3] = sw3;
	assign in_port[4] = sw4;
	
endmodule
