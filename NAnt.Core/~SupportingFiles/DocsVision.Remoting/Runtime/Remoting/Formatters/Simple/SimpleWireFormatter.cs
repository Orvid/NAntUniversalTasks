//
// Copyright (c) 2004 Digital Design. All rights reserved.
//
// Author: Vadim Skipin (skipin@digdes.com)
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace DocsVision.Runtime.Remoting.Formatters.Simple
{
	internal sealed class SimpleWireFormatter : IWireFormatter
	{
		/// <summary>
		/// Type tags
		/// </summary>
		private enum TypeId : byte
		{
			Null = 0,
			Void,
			Boolean,
			SByte,
			Byte,
			Int16,
			UInt16,
			Int32,
			UInt32,
			Int64,
			UInt64,
			Decimal,
			Single,
			Double,
			String,
			Array,
			Enum,
			Struct
		}

		/// <summary>
		/// Supported primitive types
		/// </summary>
		private struct Types
		{
			public static readonly Type typeVoid = typeof(void);
			public static readonly Type typeBoolean = typeof(Boolean);
			public static readonly Type typeByte = typeof(Byte);
			public static readonly Type typeSByte = typeof(SByte);
			public static readonly Type typeInt16 = typeof(Int16);
			public static readonly Type typeUInt16 = typeof(UInt16);
			public static readonly Type typeInt32 = typeof(Int32);
			public static readonly Type typeUInt32 = typeof(UInt32);
			public static readonly Type typeInt64 = typeof(Int64);
			public static readonly Type typeUInt64 = typeof(UInt64);
			public static readonly Type typeDecimal = typeof(Decimal);
			public static readonly Type typeSingle = typeof(Single);
			public static readonly Type typeDouble = typeof(Double);
			public static readonly Type typeGuid = typeof(Guid);
			public static readonly Type typeString = typeof(String);
			public static readonly Type typeArray = typeof(Byte[]);
		}

		#region Serialization methods

		private void SerializeObject(BinaryWriter writer, object obj)
		{
			if (obj == null)
			{
				writer.Write((byte)TypeId.Null);
				return;
			}

			Type object_type = obj.GetType();

			if (object_type.IsValueType)
			{
				if (object_type == Types.typeVoid)
				{
					writer.Write((byte)TypeId.Void);
					return;
				}
				if (object_type == Types.typeBoolean)
				{
					writer.Write((byte)TypeId.Boolean);
					writer.Write((Boolean)obj);
					return;
				}
				if (object_type == Types.typeSByte)
				{
					writer.Write((byte)TypeId.SByte);
					writer.Write((SByte)obj);
					return;
				}
				if (object_type == Types.typeByte)
				{
					writer.Write((byte)TypeId.Byte);
					writer.Write((Byte)obj);
					return;
				}
				if (object_type == Types.typeInt16)
				{
					writer.Write((byte)TypeId.Int16);
					writer.Write((Int16)obj);
					return;
				}
				if (object_type == Types.typeUInt16)
				{
					writer.Write((byte)TypeId.UInt16);
					writer.Write((UInt16)obj);
					return;
				}
				if (object_type == Types.typeInt32)
				{
					writer.Write((byte)TypeId.Int32);
					writer.Write((Int32)obj);
					return;
				}
				if (object_type == Types.typeUInt32)
				{
					writer.Write((byte)TypeId.UInt32);
					writer.Write((UInt32)obj);
					return;
				}
				if (object_type == Types.typeInt64)
				{
					writer.Write((byte)TypeId.Int64);
					writer.Write((Int64)obj);
					return;
				}
				if (object_type == Types.typeUInt64)
				{
					writer.Write((byte)TypeId.UInt64);
					writer.Write((UInt64)obj);
					return;
				}
				if (object_type == Types.typeDecimal)
				{
					writer.Write((byte)TypeId.Decimal);
					writer.Write((Decimal)obj);
					return;
				}
				if (object_type == Types.typeSingle)
				{
					writer.Write((byte)TypeId.Single);
					writer.Write((Single)obj);
					return;
				}
				if (object_type == Types.typeDouble)
				{
					writer.Write((byte)TypeId.Double);
					writer.Write((Double)obj);
					return;
				}
				if (object_type == Types.typeGuid)
				{
					writer.Write((byte)TypeId.String);
					writer.Write(((Guid)obj).ToString());
					return;
				}
				if (object_type.IsEnum)
				{
					writer.Write((byte)TypeId.Enum);
					writer.Write((Int32)obj);
					return;
				}
				if (!object_type.IsPrimitive && object_type.IsLayoutSequential)	// more complex test for structure?
				{
					writer.Write((byte)TypeId.Struct);
					SerializeStruct(writer, obj);
					return;
				}
			}
			else
			{
				if (object_type == Types.typeString)
				{
					writer.Write((byte)TypeId.String);
					writer.Write((String)obj);
					return;
				}
				if (object_type == Types.typeArray)
				{
					Byte[] arr = (Byte[])obj;
					writer.Write((byte)TypeId.Array);
					writer.Write(arr.Length);
					writer.Write(arr);
					return;
				}
			}

			throw new NotSupportedException();
		}

		private object DeserializeObject(BinaryReader reader, Type object_type)
		{
			TypeId tid = (TypeId)reader.ReadByte();
			switch (tid)
			{
				case TypeId.Null:
				case TypeId.Void:
					return null;
				case TypeId.SByte:
					return reader.ReadSByte();
				case TypeId.Byte:
					return reader.ReadByte();
				case TypeId.Boolean:
					return reader.ReadBoolean();
				case TypeId.Int16:
					return reader.ReadInt16();
				case TypeId.UInt16:
					return reader.ReadUInt16();
				case TypeId.Int32:
					return reader.ReadInt32();
				case TypeId.UInt32:
					return reader.ReadUInt32();
				case TypeId.Int64:
					return reader.ReadInt64();
				case TypeId.UInt64:
					return reader.ReadUInt64();
				case TypeId.Decimal:
					return reader.ReadDecimal();
				case TypeId.Single:
					return reader.ReadSingle();
				case TypeId.Double:
					return reader.ReadDouble();
				case TypeId.String:
					string s = reader.ReadString();
					if (object_type == Types.typeGuid)
						return new Guid(s);
					return s;
				case TypeId.Array:
					return reader.ReadBytes(reader.ReadInt32());
				case TypeId.Enum:
					int enum_value = reader.ReadInt32();
					return Enum.ToObject(object_type, enum_value);
				case TypeId.Struct:
					return DeserializeStruct(reader, object_type);
			}

			throw new NotSupportedException();
		}

		private void SerializeStruct(BinaryWriter writer, object obj)
		{
			Type struct_type = obj.GetType();
			FieldInfo[] fields = struct_type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			// fields count
			writer.Write((Int32)fields.Length);

			// fields values
			for (int i = 0; i < fields.Length; ++i)
			{
				SerializeObject(writer, fields[i].GetValue(obj));
			}
		}

		private object DeserializeStruct(BinaryReader reader, Type struct_type)
		{
			FieldInfo[] fields = struct_type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			// fields count
			int fields_count = reader.ReadInt32();
			if (fields_count > fields.Length)
				throw new RemotingException("Bad structure fields count");

			object struct_value = Activator.CreateInstance(struct_type);

			// fields values
			for (int i = 0; i < fields.Length; ++i)
			{
				fields[i].SetValue(struct_value, DeserializeObject(reader, fields[i].FieldType));
			}

			return struct_value;
		}

		#endregion

		#region IWireFormatter Members

		public void SerializeRequest(IMethodCallMessage message, ITransportHeaders requestHeaders, Stream requestStream)
		{
			BinaryWriter writer = new BinaryWriter(requestStream, Encoding.UTF8);

			// method info
			writer.Write(message.MethodName);

			// arguments count
			writer.Write((byte)message.InArgCount);

			// arguments list
			for (int i = 0; i < message.InArgCount; ++i)
			{
				SerializeObject(writer, message.GetInArg(i));
			}
		}

		public IMethodCallMessage DeserializeRequest(ITransportHeaders requestHeaders, Stream requestStream)
		{
			BinaryReader reader = new BinaryReader(requestStream, Encoding.UTF8);

			// server type
			string uri = (string)requestHeaders["__RequestUri"];
			Type svr_type = RemotingServices.GetServerTypeForUri(uri);
			if (svr_type == null)
				throw new RemotingException("No registered server for uri: " + uri);

			// method info
			string method_name = reader.ReadString();
			MethodInfo method_info = svr_type.GetMethod(method_name);
			if (method_info == null)
				throw new RemotingException("Bad method name: " + method_name);

			ParameterInfo[] parameters = method_info.GetParameters();
			Type[] method_signature = new Type[parameters.Length];

			// arguments count
			int arg_count = reader.ReadByte();
			if (arg_count > parameters.Length)
				throw new RemotingException("Bad arguments count");

			// arguments list
			object[] args = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; ++i)
			{
				method_signature[i] = parameters[i].ParameterType;
				if (!parameters[i].IsOut)
				{
					args[i] = DeserializeObject(reader, parameters[i].ParameterType);
				}
			}

			// prepare method call message
			Header[] headers = new Header[6];
			headers[0] = new Header("__Uri", uri);
			headers[1] = new Header("__TypeName", svr_type.AssemblyQualifiedName);
			headers[2] = new Header("__MethodName", method_name);
			headers[3] = new Header("__MethodSignature", method_signature);
			headers[4] = new Header("__Args", args);
			// this formatter does not support serialization of call context
			headers[5] = new Header("__CallContext", null);

			return new MethodCall(headers);
		}

		public void SerializeResponse(IMethodReturnMessage message, ITransportHeaders responseHeaders, Stream responseStream)
		{
			BinaryWriter writer = new BinaryWriter(responseStream, Encoding.UTF8);

			if (message.Exception == null)
			{
				// arguments count
				writer.Write((byte)message.OutArgCount);

				// arguments list
				for (int i = 0; i < message.OutArgCount; ++i)
				{
					SerializeObject(writer, message.GetOutArg(i));
				}

				// return value
				if ((message.MethodBase as MethodInfo).ReturnType != Types.typeVoid)
				{
					SerializeObject(writer, message.ReturnValue);
				}
			}
			else
			{
				// error marker
				writer.Write((byte)255);

				// this formatter provides very limited support for exception serialization
				writer.Write(message.Exception.Message);
			}
		}

		public IMethodReturnMessage DeserializeResponse(IMethodCallMessage message, ITransportHeaders responseHeaders, Stream responseStream)
		{
			BinaryReader reader = new BinaryReader(responseStream, Encoding.UTF8);

			// arguments count
			int arg_count = reader.ReadByte();

			// check for error marker
			if (arg_count != 255)
			{
				// method info
				MethodInfo method_info = (MethodInfo)message.MethodBase;
				ParameterInfo[] parameters = method_info.GetParameters();

				if (arg_count > parameters.Length)
					throw new RemotingException("Bad arguments count");

				// arguments list
				object[] out_args = new object[parameters.Length];
				for (int i = 0; i < parameters.Length; ++i)
				{
					if (parameters[i].IsOut)
					{
						out_args[i] = DeserializeObject(reader, parameters[i].ParameterType);
					}
				}

				// return value
				object return_value = null;
				if (method_info.ReturnType != Types.typeVoid)
				{
					return_value = DeserializeObject(reader, method_info.ReturnType);
				}

				// this formatter does not support serialization of call context
				return new ReturnMessage(return_value, out_args, arg_count, null, message);
			}
			else
			{
				// this formatter provides very limited support for exception serialization
				string error_message = reader.ReadString();
				return new ReturnMessage(new RemotingException("Server error: " + error_message), message);
			}
		}

		#endregion
	}
}