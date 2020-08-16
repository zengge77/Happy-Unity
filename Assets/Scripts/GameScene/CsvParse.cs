using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class CsvParse
{
    public static string[,] Parse(TextAsset scv)
    {
        //使用MemoryStream从内存中读取csv文件的bytes[]，因为不是从本地读取，所以不能用FileStream
        //再用StreamReader读取字符内容，进行逐行操作
        //注意，这里Encoding使用GBK，打包时需要把 Editor\Data\Mono\lib\mono\2.0 目录下的I18N.DLL 和 I18N.CJK.DLL复制到asset下
        //也可手动把csv文件储存为UTF8，避免乱码
        MemoryStream ms = new MemoryStream(scv.bytes);
        StreamReader sr = new StreamReader(ms, UnicodeEncoding.GetEncoding("GBK"));

        //获取csv的行数和列数
        string[] allDate = sr.ReadToEnd().Split('\n');//以换行符进行分割
        int height = allDate.Length - 1; //减去最后为空的一行，应该是因为csv每行结尾/r/n连用的关系
        int width = allDate[0].Split(',').Length;//第一行的列数
        sr.Close();
        ms.Close();

        //赋值
        string[,] cellType = new string[height, width];
        for (int i = 0; i < height; i++)
        {
            //删除每行最后的回车符
            allDate[i] = allDate[i].Remove(allDate[i].Length - 1);
            //以','分割数据
            string[] lineDate = allDate[i].Split(',');
            for (int j = 0; j < width; j++)
            {
                cellType[i, j] = lineDate[j];
            }
        }
        return cellType;
    }
}
