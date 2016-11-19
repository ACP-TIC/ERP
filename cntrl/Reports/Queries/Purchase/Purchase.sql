﻿select purchase_invoice.number as Number,Date(purchase_invoice.trans_date) as PurchaseDate,app_branch.name as Branch , purchase_invoice.is_impex as Import,
contacts.name as Supplier,contacts.gov_code as GovCode,contacts.code as Code , contacts.address as Address,app_currency.name as Currency, app_currencyfx.sell_value,
items.code as ItemCode, purchase_invoice_detail.item_description as ItemName,app_cost_center.name,
(select Name from item_tag_detail inner join item_tag on item_tag_detail.id_tag=Item_tag.id_tag where item_tag_detail.id_item=items.id_item order by item_tag_detail.is_default limit 0,1 ) as Tag, 
app_contract.name  as Contract,app_condition.name as SalesCondition,vatco.VatName,
quantity as Quantity, purchase_invoice_detail.unit_cost as UnitPrice,
round(( purchase_invoice_detail.unit_cost * vatco.coef),4) as UnitPriceVat,  
round((purchase_invoice_detail.quantity * purchase_invoice_detail.unit_cost),4) as SubTotal,
 round((purchase_invoice_detail.quantity * purchase_invoice_detail.unit_cost * vatco.coef),4) as SubTotalVat,
 (purchase_invoice_detail.discount) as DiscountUnitPrice,
round((purchase_invoice_detail.discount*vatco.coef),4) as DiscountUnitPriceVat,  
round((purchase_invoice_detail.quantity * (purchase_invoice_detail.discount)),4) as DiscountSubTotal,
 round((purchase_invoice_detail.quantity * (purchase_invoice_detail.discount*vatco.coef)),4) as DiscountSubTotalVat,purchase_invoice.comment
 from purchase_invoice_detail  
inner join purchase_invoice 
on purchase_invoice_detail.id_purchase_invoice=purchase_invoice.id_purchase_invoice 
inner join contacts on purchase_invoice.id_contact = contacts.id_contact  
left join items on purchase_invoice_detail.id_item = items.id_item 
LEFT OUTER JOIN 
             (SELECT app_vat_group.id_vat_group, SUM(app_vat.coefficient) + 1 AS coef ,app_vat_group.name as VatName
				FROM  app_vat_group  
					LEFT OUTER JOIN app_vat_group_details ON app_vat_group.id_vat_group = app_vat_group_details.id_vat_group  
					LEFT OUTER JOIN app_vat ON app_vat_group_details.id_vat = app_vat.id_vat  
				GROUP BY app_vat_group.id_vat_group)  
                vatco ON vatco.id_vat_group = purchase_invoice_detail.id_vat_group 
                inner join app_branch on app_branch.id_branch=purchase_invoice.id_branch
                inner join app_currencyfx on app_currencyfx.id_currencyfx=purchase_invoice.id_currencyfx
                inner join app_currency on app_currency.id_currency=app_currencyfx.id_currency
                inner join app_contract on app_contract.id_contract=purchase_invoice.id_contract
                inner join app_condition on app_condition.id_condition=purchase_invoice.id_condition
             inner join app_cost_center on app_cost_center.id_cost_center=purchase_invoice_detail.id_cost_center
 

where purchase_invoice.trans_date between @StartDate and @EndDate and purchase_invoice.id_company = @CompanyID
 order by purchase_invoice.trans_date