using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Grammar
{
    //存储文法
    class Expression
    {
        //scan print
        public string[] expression = { "S->Statement S|  Judge S|  Loop S|  Output  S|  Expression S|  Function S|  ~|  ; S|  Block S" ,

                                                         "Block->{ S }",
                                                         "Function->void identifier ( Parameter ) { S Return }",
                                                         "Type->int|  double|  float|  char",
                                                         "Parameter->Type Identifier Parameter1 Parameter2|  ~",
                                                         "Parameter1->~|  = Expression",
                                                         "Parameter2->, Type Identifier Parameter1 Parameter2|  ~",
                                                         "Return->return ;|  ~",

                                                         "Statement->int Identifier Statement1|  double Identifier Statement1|  float Identifier Statement1|  char Identifier Statement2|  boolean Identifier Statement3",
                                                         "Assignment->identifier Statement4 Arithmetic1|  ~",
                                                         "Arithmetic1->, Identifier Statement4 Arithmetic1|  ~",
                                                         "Statement1->Arithmetic1 ;|  [ List Arithmetic1 ;|  = Expression Arithmetic1 ;|  ( Parameter ) { S Return1 }",
                                                         "Statement4->~|  [ List|  = Statement7",
                                                         "Statement7->Expression|  ~",
                                                         "List->Expression ] Statement4 Array|  ] = { Expression Constant1 } ",
                                                         "Array->= { Set }|  ~",
                                                         "Set->, { Set } Set|  { Set } Set|  Expression Constant1 Set|  ~",
                                                         "Return1->return Expression ;|  ~",

                                                         "Constant1->~|  , Expression Constant1",
                                                         "Output->printf ( Expression ) ;",
                                                         "Input->scanf ( identifier )",

                                                         "Statement2->Arithmetic2 ;|  [ List2 Arithmetic2 ;|  = Expression Arithmetic2 ;|  ( Parameter ) { S Return1 }",
                                                         "Statement5->~|  [ List2|  = Expression",
                                                         "List2->Expression ]  Array1|  ] = Array2",
                                                         "Arithmetic2->, Identifier Statement5 Arithmetic2|  ~",
                                                         "Array1->= Array2|  ~",
                                                         "Array2->Expression|  { Expression Constant1 }",

                                                         "Arithmetic3->, Identifier Statement6 Arithmetic3|  ~",
                                                         "Statement3->Arithmetic3 ;|  [ List3 Arithmetic3 ;|  = Bool Arithmetic3 ;|  ( Parameter ) { S Return1 }",
                                                         "Statement6->~|  [ List3|  = Bool",
                                                         "List3->Expression ] Array3|  ] = { Bool Bool1 }",
                                                         "Array3->= { Bool Bool1 }|  ~",
                                                         "Bool->true|  false|  Identifier|  Expression",
                                                         "Bool1->, Bool Bool1|  ~",
                                                         "Identifier->identifier",

                                                         "Expression->E11 E12",
                                                         "E11->E1 E2",
                                                         "E12->= E11 E12|  ~",
                                                         "E1->E3 E4",
                                                         "E2->&& E1 E2|  || E1 E2|  ~",
                                                         "E3->E5 E6",
                                                         "E4->< E3 E4|  <= E3 E4|  > E3 E4|  >= E3 E4|  == E3 E4|  <> E3 E4|  ~",
                                                         "E5->E7 E8",
                                                         "E6->+ E5 E6|  - E5 E6|  ~",
                                                         "E7->! E9|  E9",
                                                         "E8->* E7 E8|  / E7 E8|  % E7 E8|  ~",
                                                         "E9->( Expression )|  constant|  identifier E10|  Input",
                                                         "E10->[ Expression ]|  ( Value Value1 )|  ~",

                                                         "Value->constant|  identifier|  ~",
                                                         "Value1->, Value Value1|  ~",
                                                          
                                                         "Jump->continue ;|  break ;",
                                                         "Jump1->Jump|  ~",
                                                         "Judge->if ( Expression ) Judge5",
                                                         "Judge5->if ( Expression ) Judge5|  Judge4",
                                                         "Context->{ S Jump1 }|  Statement|  Expression ;|  Loop|  Output|  ;|  Jump",
                                                         "Judge4->Context Judge2",
                                                         "Judge2->else Judge3|  ~",
                                                         "Judge3->if ( Expression ) Context Judge2|  Context",

                                                         "Operator-><|  >=|  >|  <=|  <>|  ==",
                                                         "Loop->while ( Expression )  Loop1|  do Loop2|  for ( Loop3 Loop5 ; Assignment ) Loop4",
                                                         "Loop1->while ( Expression )  Loop1|  Statement|  Expression|  Judge|  Output|  { S }|  ;",
                                                         "Loop2->do Loop2|  { S } while ( Expression ) ;|  Statement while ( Expression ) ;|  Expression while ( Expression ) ;|  Judge while ( Expression ) ;",
                                                         "Loop3->Assignment|  Statement|  ;",
                                                         "Loop5->Expression|  ~",
                                                         "Loop4->for ( Loop3 Loop5 ; Assignment ) Loop4|  { S }|  Statement|  Expression|  Output|  Judge|  ;"
                                                         };
    }
}