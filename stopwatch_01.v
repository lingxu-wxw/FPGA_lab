//主模块
module stopwatch_01(clk, key_reset, key_start_pause, key_display_stop,
                    hex0, hex1, hex2, hex3, hex4, hex5,
                    led0, led1, led2);
input        clk, key_reset, key_start_pause, key_display_stop;
output [6:0] hex0, hex1, hex2, hex3, hex4, hex5;
output       led0, led1, led2;
reg led0, led1, led2;

parameter DELAY_TIME = 5000000;  //消抖动延迟时间

reg display_work;   //显示暂停标记
reg counter_work;   //计时暂停标记

reg [3:0] minute_display_high;
reg [3:0] minute_display_low;
reg [3:0] second_display_high;
reg [3:0] second_display_low;
reg [3:0] msecond_display_high;
reg [3:0] msecond_display_low;

reg [3:0] minute_counter_high;
reg [3:0] minute_counter_low;
reg [3:0] second_counter_high;
reg [3:0] second_counter_low;
reg [3:0] msecond_counter_high;
reg [3:0] msecond_counter_low;

reg [31:0] counter_50M;

reg reset_1_time;   //复位键状态暂存器
reg [31:0] counter_reset;   //复位键消抖计时器
reg start_1_time;   //计时键状态暂存器
reg [31:0] counter_start;   //计时键消抖计时器
reg display_1_time;   //显示键状态暂存器
reg [31:0] counter_display;   //显示键消抖计时器

sevenseg LED8_minute_display_high(minute_display_high, hex5);
sevenseg LED8_minute_display_low(minute_display_low, hex4);

sevenseg LED8_second_display_high(second_display_high, hex3);
sevenseg LED8_second_display_low(second_display_low, hex2);

sevenseg LED8_msecond_display_high(msecond_display_high, hex1);
sevenseg LED8_msecond_display_low(msecond_display_low, hex0);


//当复位键被按下时，led0亮起
always @ (key_reset) 
begin
    led0 = !key_reset;
end

//当计时键被按下时，led1亮起
always @ (key_start_pause) 
begin
    led1 = !key_start_pause;
end

//当显示键被按下时，led2亮起
always @ (key_display_stop) 
begin
    led2 = !key_display_stop;
end

always @ (posedge clk) 
begin
//当复位键被持续按下一个 DELAY_TIME的时间后，则认为该次按键为有效，进行“复位”操作
	if (reset_1_time && !key_reset) 
	begin
		counter_reset = counter_reset + 1;
		if (counter_reset == DELAY_TIME) 
		begin
			counter_reset = 0;
			reset_1_time = ~reset_1_time;

			counter_work = 0;
			minute_counter_high = 0;
			minute_counter_low = 0;
			second_counter_high = 0;
			second_counter_low = 0;
			msecond_counter_high = 0;
			msecond_counter_low = 0;
			
			display_work = 1;
		end
   end 
//两次复位键被按下应相隔一个 DELAY_TIME时间，以消除电路延迟影响，然后消除复位键暂存状态
	else if (!reset_1_time && key_reset) 
	begin
		counter_reset = counter_reset + 1;
		if (counter_reset == DELAY_TIME) 
		begin
			counter_reset = 0;
			reset_1_time = ~reset_1_time;
		end
   end 
	else 
	begin
		counter_reset = 0;
	end
	
//当计时键被持续按下一个 DELAY_TIME的时间后，则认为该次按键为有效，进行“计时开始/暂停”操作
	if (start_1_time && !key_start_pause) 
	begin
		counter_start = counter_start + 1;
		if (counter_start == DELAY_TIME) 
		begin
			counter_start = 0;
			start_1_time = ~start_1_time;

			counter_work = !counter_work;
		end
   end 
//两次计时键被按下应相隔一个 DELAY_TIME时间，以消除电路延迟影响，然后消除计时键暂存状态
	else if (!start_1_time && key_start_pause) 
	begin
		counter_start = counter_start + 1;
		if (counter_start == DELAY_TIME) 
		begin
			counter_start = 0;
			start_1_time = ~start_1_time;
		end
   end 
	else 
	begin
		counter_start = 0;
   end
	
//当显示被持续按下一个 DELAY_TIME的时间后，则认为该次按键为有效，进行“显示开始/暂停”操作
	if (display_1_time && !key_display_stop) 
	begin
		counter_display = counter_display + 1;
		if (counter_display == DELAY_TIME) 
		begin
			counter_display = 0;
			display_1_time = ~display_1_time;
      end
   end 
//两次显示键被按下应相隔一个 DELAY_TIME时间，以消除电路延迟影响，然后消除显示键暂存状态，并在一开始的时候对display_work进行初始化
	else if (!display_1_time && key_display_stop) 
	begin
		counter_display = counter_display + 1;
      if (counter_display == DELAY_TIME) 
begin
			counter_display = 0;
			display_1_time = ~display_1_time;

          display_work = !display_work;
      end
   end 
	else 
	begin
		counter_display = 0;
   end
	
//如果显示处于开启状态，则显示时间等于计时时间，否则不做任何改变
	if (display_work) 
	begin
		minute_display_high = minute_counter_high;
		minute_display_low = minute_counter_low;
		second_display_high = second_counter_high;
		second_display_low = second_counter_low;
		msecond_display_high = msecond_counter_high;
		msecond_display_low = msecond_counter_low;
	end

//如果计时处于开启状态，则根据时钟的时间进行计时。有需要时，分、秒、毫秒进行相应的进位操作
	if (counter_work) 
	begin
		counter_50M = counter_50M + 1;

		if (counter_50M == 500000) 
		begin
			counter_50M = 0;
         msecond_counter_low = msecond_counter_low + 1;

         if (msecond_counter_low == 10) 
			begin
				msecond_counter_low = 0;
				msecond_counter_high = msecond_counter_high + 1;

				if (msecond_counter_high == 10) 
				begin
					msecond_counter_high = 0;
					second_counter_low = second_counter_low + 1;

					if (second_counter_low == 10) 
					begin
						second_counter_low = 0;
						second_counter_high = second_counter_high + 1;

						if (second_counter_high == 6) 
						begin
							second_counter_high = 0;
							minute_counter_low = minute_counter_low + 1;

							if (minute_counter_low == 10) 
							begin
								minute_counter_low = 0;
								minute_counter_high = minute_counter_high + 1;

								if (minute_counter_high == 10) 
								begin
									minute_counter_high = 0;
                        end
                     end
						end
					end
				end
			end
		end
	end
end
endmodule

//七段数码管模块
module sevenseg(data, ledsegments);
input [3:0] data;
output [6:0] ledsegments;
reg [6:0] ledsegments;
always @ ( * )
    case(data)
        0: ledsegments = 7'b100_0000;
        1: ledsegments = 7'b111_1001;
        2: ledsegments = 7'b010_0100;
        3: ledsegments = 7'b011_0000; 
        4: ledsegments = 7'b001_1001;
        5: ledsegments = 7'b001_0010;
        6: ledsegments = 7'b000_0010;
        7: ledsegments = 7'b111_1000;
        8: ledsegments = 7'b000_0000;
        9: ledsegments = 7'b001_0000;
        default: ledsegments = 7'b111_1111;
    endcase
endmodule 

