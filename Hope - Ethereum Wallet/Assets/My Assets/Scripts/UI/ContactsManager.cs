﻿using System.Collections.Generic;

public sealed class ContactsManager
{

	public Dictionary<string, string> Contacts { get; } = new Dictionary<string, string>();

	public Dictionary<string, int> ContactOrders { get; } = new Dictionary<string, int>();

	public ContactsManager()
	{
		SetContacts();
	}

	public void AddContact(string contactAddress, string contactName, int contactsCount)
	{
		Contacts.Add(contactAddress, contactName);
		ContactOrders.Add(contactAddress, contactsCount);
	}

	public void RemoveContact(string contactAddress)
	{
		Contacts.Remove(contactAddress);
		ContactOrders.Remove(contactAddress);
	}

	public void EditContact(string newContactAddress, string previousAddress, string newContactName, int index)
	{
		Contacts.Remove(previousAddress);
		ContactOrders.Remove(previousAddress);
		Contacts.Add(newContactAddress, newContactName);
		ContactOrders.Add(newContactAddress, index);
	}

	/// <summary>
	/// Sets all the contacts from the SecurePlayerPrefs to the Contacts dictionary
	/// </summary>
	private void SetContacts()
	{
		for (int i = 1; ; i++)
		{
			if (!SecurePlayerPrefs.HasKey("contact_" + i))
				return;

			string address = SecurePlayerPrefs.GetString("contact_" + i);
			Contacts.Add(address, SecurePlayerPrefs.GetString(address));
			ContactOrders.Add(address, i);
		}
	}
}
